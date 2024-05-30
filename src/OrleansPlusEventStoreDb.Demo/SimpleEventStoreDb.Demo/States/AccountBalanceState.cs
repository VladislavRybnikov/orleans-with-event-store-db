using SimpleEventStoreDb.Demo.Aggregates;
using SimpleEventStoreDb.Demo.Events;

namespace SimpleEventStoreDb.Demo.States;

public record AccountBalanceState : IState<AccountBalanceState>,
    IApplicable<AccountCreated>,
    IApplicable<Deposit>,
    IApplicable<Withdrawal>
{
    public int Version { get; set; }

    public decimal Amount { get; set; }

    public string? Currency { get; set; }

    public static AccountBalanceState Empty => new();

    public void Apply(AccountCreated domainEvent)
    {
        Amount = domainEvent.InitialBalance;
        Currency = domainEvent.Currency;
        Version = domainEvent.Version;
    }

    public void Apply(Deposit domainEvent)
    {
        Amount += domainEvent.Amount;
        Version = domainEvent.Version;
    }

    public void Apply(Withdrawal domainEvent)
    {
        Amount -= domainEvent.Amount;
        Version = domainEvent.Version;
    }

    // not used
    public void Apply(IDomainEvent domainEvent)
    {
        Version = domainEvent.Version;
        switch (domainEvent)
        {
            case AccountCreated accountCreated:
                Amount = accountCreated.InitialBalance;
                Currency = accountCreated.Currency;
                break;
            
            case Deposit deposit:
                Amount += deposit.Amount;
                break;
                
            case Withdrawal withdrawal:
                Amount -= withdrawal.Amount;
                break;
        }
    }
}