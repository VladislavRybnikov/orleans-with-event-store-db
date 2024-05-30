namespace SimpleEventStoreDb.Demo.Events;

public record AccountCreated(
    Guid Id, 
    string Currency, 
    decimal InitialBalance = 0, 
    int Version = 0) : IDomainEvent;