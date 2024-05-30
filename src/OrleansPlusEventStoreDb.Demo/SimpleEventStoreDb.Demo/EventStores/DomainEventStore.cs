using EventStore.Client;
using SimpleEventStoreDb.Demo.Events;

namespace SimpleEventStoreDb.Demo.EventStores;

public class DomainEventStore<TAggregate>(EventStoreClient eventStoreClient) : IDomainEventStore<TAggregate>
{
    private static readonly Type AggregateType = typeof(TAggregate);

    public async Task<IEnumerable<IDomainEvent>> ReadAsync(string id)
    {
        var streamResult = eventStoreClient.ReadStreamAsync(
            Direction.Forwards,
            $"{AggregateType.Name}-{id}",
            StreamPosition.Start);

        if (await streamResult.ReadState != ReadState.Ok) return ArraySegment<IDomainEvent>.Empty;

        var domainEvents = await streamResult.Select(e => e.Event.MapToDomainEvent()).ToListAsync();
        return domainEvents.Where(x => x != null).Select(x => x!);
    }

    public async Task SaveAsync(string id, int version, IEnumerable<IDomainEvent> domainEvents)
    {
        var eventsData = domainEvents.Select(x => x.GetEventData()).ToArray();

        await eventStoreClient.AppendToStreamAsync(
            $"{AggregateType.Name}-{id}",
            version == 0 ? StreamRevision.None : StreamRevision.FromInt64(version - 1), 
            eventsData);
    }
}
