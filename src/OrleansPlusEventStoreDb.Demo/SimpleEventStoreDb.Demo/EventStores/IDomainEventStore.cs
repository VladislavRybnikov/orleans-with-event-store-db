using SimpleEventStoreDb.Demo.Events;

namespace SimpleEventStoreDb.Demo.EventStores;

public interface IDomainEventStore<TAggregate>
{
    Task<IEnumerable<IDomainEvent>> ReadAsync(string id);

    Task SaveAsync(string id, int version, IEnumerable<IDomainEvent> domainEvents);
}