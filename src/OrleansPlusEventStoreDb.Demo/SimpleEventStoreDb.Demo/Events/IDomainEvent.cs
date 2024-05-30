namespace SimpleEventStoreDb.Demo.Events;

public interface IDomainEvent
{
    Guid Id { get; }
    int Version { get; }
}