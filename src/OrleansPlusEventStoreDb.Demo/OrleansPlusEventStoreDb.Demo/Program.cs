using EventStore.Client;
using Microsoft.AspNetCore.Mvc;
using OrleansPlusEventStoreDb.Demo;
using OrleansPlusEventStoreDb.Demo.Grains;
using OrleansPlusEventStoreDb.Demo.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

const string eventStoreConnectionString = "esdb://admin:changeit@localhost:2113?tls=false&tlsVerifyCert=false";

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services
    .AddEventStoreClient(eventStoreConnectionString)
    .AddEventStorePersistentSubscriptionsClient(eventStoreConnectionString)
    .AddEventStoreProjectionManagementClient(eventStoreConnectionString);

builder.Services.AddTransient(typeof(IStateRepository<>), typeof(StateRepository<>));

builder.Services.AddHostedService<EventStoreHostedService>();

builder.Host.UseOrleans(static siloBuilder =>
{
    siloBuilder.UseLocalhostClustering();
    siloBuilder.AddMemoryGrainStorage("accounts");
    siloBuilder.AddCustomStorageBasedLogConsistencyProviderAsDefault();
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/accounts/{accountId:guid}", async (Guid accountId, IGrainFactory grainFactory) =>
    {
        var accountGrain = grainFactory.GetGrain<IAccountBalanceGrain>(accountId);
        return await accountGrain.GetBalance();
    })
    .WithName("GetAccountBalance")
    .WithOpenApi();

app.MapPost("accounts", async (IGrainFactory grainFactory, CreateAccountRequest request) =>
    {
        var accountId = Guid.NewGuid();
        var accountGrain = grainFactory.GetGrain<IAccountBalanceGrain>(accountId);
        var result = await accountGrain.CreateAccount(request.Currency, 0);

        return !result.IsSuccess ? Results.BadRequest(result.ErrorMessage) : Results.Ok(accountId);
    })
    .WithName("CreateAccount")
    .WithOpenApi();

app.MapPost("/accounts/{accountId:guid}/deposits", async (Guid accountId, [FromQuery] decimal amount, IGrainFactory grainFactory) =>
        {
            var accountGrain = grainFactory.GetGrain<IAccountBalanceGrain>(accountId);
            var result = await accountGrain.Deposit(amount);
            
            return !result.IsSuccess ? Results.BadRequest(result.ErrorMessage) : Results.Ok();
        })
    .WithName("CreateDeposit")
    .WithOpenApi();

app.MapPost("/accounts/{accountId:guid}/withdrawals", async (Guid accountId, [FromQuery] decimal amount, IGrainFactory grainFactory) =>
    {
        var accountGrain = grainFactory.GetGrain<IAccountBalanceGrain>(accountId);
        var result = await accountGrain.Withdraw(amount);
        
        return !result.IsSuccess ? Results.BadRequest(result.ErrorMessage) : Results.Ok();
    })
    .WithName("CreateWithdrawal")
    .WithOpenApi();

app.Run();

record CreateAccountRequest(string Currency);