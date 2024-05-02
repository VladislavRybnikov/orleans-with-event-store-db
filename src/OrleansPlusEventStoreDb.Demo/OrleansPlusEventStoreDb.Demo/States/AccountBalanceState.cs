using OrleansPlusEventStoreDb.Demo.Events;

namespace OrleansPlusEventStoreDb.Demo.States;

[StateSnapshot(SnapshotPerEventsNumber = 2)]
public class AccountBalanceState : IState<AccountBalanceState>
{
    public decimal Amount { get; set; }

    public string Currency { get; set; }

    public int Version { get; set; }

    public static AccountBalanceState Empty => new();

    public AccountBalanceState Apply(AccountCreated created)
    {
        Amount = created.InitialBalance;
        Currency = created.Currency;
        Version = created.Version;

        return this;
    }

    public AccountBalanceState Apply(Withdrawal withdrawal)
    {
        Amount -= withdrawal.Amount;
        Version = withdrawal.Version;

        return this;
    }
    
    public AccountBalanceState Apply(Deposit deposit)
    {
        Amount += deposit.Amount;
        Version = deposit.Version;

        return this;
    }
}