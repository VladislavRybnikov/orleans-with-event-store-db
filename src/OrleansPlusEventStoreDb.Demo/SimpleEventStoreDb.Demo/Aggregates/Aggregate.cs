using SimpleEventStoreDb.Demo.Events;
using SimpleEventStoreDb.Demo.States;

namespace SimpleEventStoreDb.Demo.Aggregates;

public abstract class Aggregate<TState>(Guid id, TState? initialState = null, int version = 0) 
    : IAggregate<TState> 
    where TState : class, IState<TState>
{
    private readonly List<IDomainEvent> _uncommittedEvents = [];

    public IEnumerable<IDomainEvent> UncommittedEvents => _uncommittedEvents;

    public TState State { get; } = initialState ?? TState.Empty;

    public Guid Id { get; } = id;

    public int Version { get; private set; } = version;

    public int NextVersion => Version + 1;

    public void ClearUncommittedEvents()
    {
        _uncommittedEvents.Clear();
    }

    protected void Emit<TDomainEvent>(TDomainEvent domainEvent) where TDomainEvent : IDomainEvent
    {
        _uncommittedEvents.Add(domainEvent);
        if (State is IApplicable<TDomainEvent> applicable)
        {
            applicable.Apply(domainEvent);
            Version = domainEvent.Version;
        }
    }
}