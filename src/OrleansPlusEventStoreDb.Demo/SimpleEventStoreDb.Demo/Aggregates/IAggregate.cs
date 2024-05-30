using SimpleEventStoreDb.Demo.Events;
using SimpleEventStoreDb.Demo.States;

namespace SimpleEventStoreDb.Demo.Aggregates;

public interface IAggregate<out TThis, TState> : IAggregate<TState>
    where TThis : IAggregate<TState>, IAggregate<TThis, TState>
    where TState : IState<TState>
{
    static abstract TThis Load(Guid id, TState state, int version);
    
    IEnumerable<IDomainEvent> UncommittedEvents { get; }

    void ClearUncommittedEvents();
}

public interface IAggregate<out TState> 
    where TState : IState<TState>
{
    Guid Id { get; }

    int Version { get; }

    TState State { get; }
}