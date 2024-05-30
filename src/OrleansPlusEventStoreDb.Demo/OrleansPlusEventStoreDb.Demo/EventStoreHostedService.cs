using EventStore.Client;
using OrleansPlusEventStoreDb.Demo.Models;
using OrleansPlusEventStoreDb.Demo.States;

namespace OrleansPlusEventStoreDb.Demo;

public class EventStoreHostedService(
    EventStoreProjectionManagementClient eventStoreProjectionManagementClient,
    EventStorePersistentSubscriptionsClient eventStorePersistentSubscriptionsClient,
    EventStoreClient eventStoreClient,
    ILogger<EventStoreHostedService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await eventStoreProjectionManagementClient.EnableAsync("$by_category", cancellationToken: stoppingToken);

        var events = await SubscribeAsync(
            $"$ce-{nameof(AccountBalanceState)}", 
            group: "Worker",
            cancellationToken: stoppingToken);

        await foreach (var message in events.WithCancellation(stoppingToken))
        {
            var aggregateId = Guid.Parse(message.Event.EventStreamId.Split('-')[1]);

            var integrationEvent = new IntegrationEvent(aggregateId.ToString(), message.Event.MapToDomainEvent()!);
            
            logger.LogInformation("Integration event: {DomainEvent}", integrationEvent.ToString());
        }
    }

    private async Task<IAsyncEnumerable<ResolvedEvent>> SubscribeAsync(string stream, string? group = null, long? fromPosition = null, CancellationToken cancellationToken = default)
    {
        if (group != null)
        {
            return await SubscribeWithPersistentSubscriptionAsync(stream, group, cancellationToken);
        }

        return eventStoreClient.SubscribeToStream(
            stream, 
            fromPosition.HasValue
                ? FromStream.After(StreamPosition.FromInt64(fromPosition.Value))
                : FromStream.End,
            resolveLinkTos: true,
            cancellationToken: cancellationToken);
    }

    private async Task<IAsyncEnumerable<ResolvedEvent>> SubscribeWithPersistentSubscriptionAsync(string stream, string groupName, CancellationToken cancellationToken)
    {
        var existingGroups = (await eventStorePersistentSubscriptionsClient
                .ListToStreamAsync(stream, cancellationToken: cancellationToken))
            .Select(x => x.GroupName)
            .ToHashSet();

        if (!existingGroups.Contains(groupName))
        {
            await eventStorePersistentSubscriptionsClient.CreateToStreamAsync(
                stream,
                groupName, 
                new PersistentSubscriptionSettings(resolveLinkTos: true),
                cancellationToken: cancellationToken);
        }
        
        return eventStorePersistentSubscriptionsClient.SubscribeToStream(
            stream, 
            groupName,
            cancellationToken: cancellationToken);
    }
}