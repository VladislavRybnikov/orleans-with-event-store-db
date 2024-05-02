using OrleansPlusEventStoreDb.Demo.Events;

namespace OrleansPlusEventStoreDb.Demo.Models;

public record IntegrationEvent(string AggregateId, IDomainEvent DomainEvent);