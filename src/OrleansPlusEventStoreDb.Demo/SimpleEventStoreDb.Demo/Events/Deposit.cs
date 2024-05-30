namespace SimpleEventStoreDb.Demo.Events;

public record Deposit(
    Guid Id, 
    decimal Amount, 
    int Version) : IDomainEvent;