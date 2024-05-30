using System.Collections.Immutable;
using EventStore.Client;
using Grpc.Core;
using SimpleEventStoreDb.Demo.Aggregates;

namespace SimpleEventStoreDb.Demo;

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
            $"$ce-{nameof(AccountBalance)}", 
            group: "Worker",
            cancellationToken: stoppingToken);

        await foreach (var message in events.WithCancellation(stoppingToken))
        {
            var aggregateId = Guid.Parse(GetEventId(message));

            var integrationEvent = new IntegrationEvent(aggregateId.ToString(), message.Event.MapToDomainEvent()!);
            
            logger.LogInformation("Integration event: {DomainEvent}", integrationEvent.ToString());
        }
    }

    private static string GetEventId(ResolvedEvent message)
    {
        var streamId = message.Event.EventStreamId.AsSpan();
        return streamId[(streamId.IndexOf('-')+1)..].ToString();
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
        try
        {
            await eventStorePersistentSubscriptionsClient.CreateToStreamAsync(
                stream,
                groupName,
                new PersistentSubscriptionSettings(resolveLinkTos: true),
                cancellationToken: cancellationToken);
        }
        catch (RpcException rpcException) when (rpcException.StatusCode == StatusCode.AlreadyExists)
        {
            // ignore
        }

        return eventStorePersistentSubscriptionsClient.SubscribeToStream(
            stream, 
            groupName,
            cancellationToken: cancellationToken);
    }
}