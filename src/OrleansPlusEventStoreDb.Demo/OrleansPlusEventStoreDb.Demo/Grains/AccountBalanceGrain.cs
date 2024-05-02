using Orleans.EventSourcing;
using Orleans.EventSourcing.CustomStorage;
using OrleansPlusEventStoreDb.Demo.Events;
using OrleansPlusEventStoreDb.Demo.Models;
using OrleansPlusEventStoreDb.Demo.Repositories;
using OrleansPlusEventStoreDb.Demo.States;

namespace OrleansPlusEventStoreDb.Demo.Grains;

public class AccountBalanceGrain(IStateRepository<AccountBalanceState> stateRepository) :
    JournaledGrain<AccountBalanceState, IDomainEvent>,
    IAccountBalanceGrain,
    ICustomStorageInterface<AccountBalanceState, IDomainEvent>
{
    private int NextVersion => Version + 1;

    public async Task<ProcessingResult> CreateAccount(string currency, decimal initialBalance)
    {
        if (Version > 0)
        {
            return ProcessingResult.Fail("Account already exists");
        }

        RaiseEvent(new AccountCreated(Guid.NewGuid(), currency, initialBalance, NextVersion));
        await ConfirmEvents();
        return ProcessingResult.Success();
    }

    public async Task<ProcessingResult> Deposit(decimal amount)
    {
        if (Version == 0)
        {
            return ProcessingResult.Fail("Account does not exist");
        }

        RaiseEvent(new Deposit(Guid.NewGuid(), amount, NextVersion));
        await ConfirmEvents();
        return ProcessingResult.Success();
    }

    public async Task<ProcessingResult> Withdraw(decimal amount)
    {
        if (Version == 0)
        {
            return ProcessingResult.Fail("Account does not exist");
        }

        if (State.Amount < amount)
        {
            return ProcessingResult.Fail("Not enough money");
        }

        RaiseEvent(new Withdrawal(Guid.NewGuid(), amount, NextVersion));
        await ConfirmEvents();
        return ProcessingResult.Success();
    }

    public Task<Balance> GetBalance()
    {
        return Task.FromResult(new Balance(State));
    }

    public async Task<KeyValuePair<int, AccountBalanceState>> ReadStateFromStorage()
    {
        var state = await stateRepository.LoadAsync(this.GetPrimaryKeyString());
        return new KeyValuePair<int, AccountBalanceState>(state.Version, state);
    }

    public async Task<bool> ApplyUpdatesToStorage(IReadOnlyList<IDomainEvent> updates, int expectedVersion)
    {
        await stateRepository.SaveAsync(this.GetPrimaryKeyString(), expectedVersion, State, updates);
        return true;
    }
}