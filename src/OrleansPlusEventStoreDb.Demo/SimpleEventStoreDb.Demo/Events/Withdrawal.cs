namespace SimpleEventStoreDb.Demo.Events;

public record Withdrawal(
    Guid Id, 
    decimal Amount, 
    int Version) : IDomainEvent;