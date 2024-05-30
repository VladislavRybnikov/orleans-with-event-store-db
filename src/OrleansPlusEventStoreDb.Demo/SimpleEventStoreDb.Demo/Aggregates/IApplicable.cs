using SimpleEventStoreDb.Demo.Events;

namespace SimpleEventStoreDb.Demo.Aggregates;

public interface IApplicable<in TDomainEvent> where TDomainEvent : IDomainEvent
{
    void Apply(TDomainEvent domainEvent);
}