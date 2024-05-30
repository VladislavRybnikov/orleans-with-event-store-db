using SimpleEventStoreDb.Demo.Events;

namespace SimpleEventStoreDb.Demo;

public record IntegrationEvent(string AggregateId, IDomainEvent DomainEvent);