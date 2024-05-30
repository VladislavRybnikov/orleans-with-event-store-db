using SimpleEventStoreDb.Demo.Events;
using SimpleEventStoreDb.Demo.States;

namespace SimpleEventStoreDb.Demo.Aggregates;

public class AccountBalance(Guid id, AccountBalanceState? initialState = null, int version = 0) : 
    Aggregate<AccountBalanceState>(id, initialState, version),
    IAggregate<AccountBalance, AccountBalanceState>
{
    public static AccountBalance Load(Guid id, AccountBalanceState state, int version) => new(id, state, version);
    
    public void CreateAccount(
        string currency,
        decimal initialBalance = 0)
    {
        if (Version > 0)
        {
            throw new Exception("Account already exists");
        }

        Emit(new AccountCreated(Id, currency, initialBalance, NextVersion));
    }

    public void Deposit(decimal amount)
    {
        if (Version == 0)
        {
            throw new Exception("Account does not exist");
        }
        
        Emit(new Deposit(Id, amount, NextVersion));
    }
    
    public void Withdrawal(decimal amount)
    {
        if (Version == 0)
        {
            throw new Exception("Account does not exist");
        }
        
        if (State.Amount < amount)
        {
            throw new Exception("Not enough money");
        }
        
        Emit(new Withdrawal(Id, amount, NextVersion));
    }
}