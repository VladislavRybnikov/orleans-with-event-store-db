using SimpleEventStoreDb.Demo.Aggregates;
using SimpleEventStoreDb.Demo.EventStores;
using SimpleEventStoreDb.Demo.States;

namespace SimpleEventStoreDb.Demo;

public interface IAggregateManager<TAggregate, TState> 
    where TAggregate : IAggregate<TAggregate, TState>
    where TState : IState<TState>
{
    Task<TAggregate> LoadAsync(Guid id);
    Task SaveAsync(TAggregate aggregate);
}

public class AggregateManager<TAggregate, TState>(IDomainEventStore<TAggregate> domainEventStore) : IAggregateManager<TAggregate, TState>    
    where TAggregate : IAggregate<TAggregate, TState>
    where TState : IState<TState>

{
    public async Task<TAggregate> LoadAsync(Guid id)
    {
        var state = TState.Empty;
        var domainEvents = await domainEventStore.ReadAsync(id.ToString());
        state.ApplyDomainEvents(domainEvents);
        return TAggregate.Load(id, state, state.Version);
    }

    public async Task SaveAsync(TAggregate aggregate)
    {
        var expectedVersion = aggregate.UncommittedEvents.MinBy(x => x.Version)!.Version - 1;
        await domainEventStore.SaveAsync(aggregate.Id.ToString(), expectedVersion, aggregate.UncommittedEvents);
        aggregate.ClearUncommittedEvents();
    }
}