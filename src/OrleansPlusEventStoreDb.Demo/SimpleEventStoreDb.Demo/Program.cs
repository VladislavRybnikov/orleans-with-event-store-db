using Microsoft.AspNetCore.Mvc;
using SimpleEventStoreDb.Demo;
using SimpleEventStoreDb.Demo.Aggregates;
using SimpleEventStoreDb.Demo.EventStores;
using SimpleEventStoreDb.Demo.States;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

const string eventStoreConnectionString = "esdb://admin:changeit@localhost:2113?tls=false&tlsVerifyCert=false";
builder.Services
    .AddEventStoreClient(eventStoreConnectionString)
    .AddEventStorePersistentSubscriptionsClient(eventStoreConnectionString)
    .AddEventStoreProjectionManagementClient(eventStoreConnectionString);

builder.Services.AddTransient(typeof(IDomainEventStore<>), typeof(DomainEventStore<>));
builder.Services.AddTransient(typeof(IAggregateManager<,>), typeof(AggregateManager<,>));

builder.Services.AddHostedService<EventStoreHostedService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/accounts/{accountId:guid}", async (
        Guid accountId, 
        IAggregateManager<AccountBalance, AccountBalanceState> aggregateManager) =>
    {
        var aggregate = await aggregateManager.LoadAsync(accountId);
        return aggregate.State;
    })
    .WithName("GetAccountBalance")
    .WithOpenApi();

app.MapPost("accounts", async (
        IAggregateManager<AccountBalance, AccountBalanceState> aggregateManager, 
        CreateAccountRequest request) =>
    {
        var accountId = Guid.NewGuid();
        var aggregate = await aggregateManager.LoadAsync(accountId);
        aggregate.CreateAccount(request.Currency);
        await aggregateManager.SaveAsync(aggregate);

        return Results.Ok(accountId);
    })
    .WithName("CreateAccount")
    .WithOpenApi();

app.MapPost("/accounts/{accountId:guid}/deposits", async (
        Guid accountId, 
        [FromQuery] decimal amount, 
        IAggregateManager<AccountBalance, AccountBalanceState> aggregateManager) =>
    {
        var aggregate = await aggregateManager.LoadAsync(accountId);
        aggregate.Deposit(amount);
        await aggregateManager.SaveAsync(aggregate);

        return Results.Ok(accountId);
    })
    .WithName("CreateDeposit")
    .WithOpenApi();

app.MapPost("/accounts/{accountId:guid}/withdrawals", async (
        Guid accountId, 
        [FromQuery] decimal amount, 
        IAggregateManager<AccountBalance, AccountBalanceState> aggregateManager) =>
    {
        var aggregate = await aggregateManager.LoadAsync(accountId);
        aggregate.Withdrawal(amount);
        await aggregateManager.SaveAsync(aggregate);

        return Results.Ok(accountId);
    })
    .WithName("CreateWithdrawal")
    .WithOpenApi();

app.Run();

record CreateAccountRequest(string Currency);