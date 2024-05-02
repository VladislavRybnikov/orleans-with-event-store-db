using OrleansPlusEventStoreDb.Demo.States;

namespace OrleansPlusEventStoreDb.Demo.Models;

[GenerateSerializer]
[Alias("OrleansPlusEventStoreDb.Demo.Models.Balance")]
public record Balance(string Currency, decimal Amount)
{
    public Balance(AccountBalanceState accountBalanceState) : this(accountBalanceState.Currency,
        accountBalanceState.Amount)
    {
    }
}