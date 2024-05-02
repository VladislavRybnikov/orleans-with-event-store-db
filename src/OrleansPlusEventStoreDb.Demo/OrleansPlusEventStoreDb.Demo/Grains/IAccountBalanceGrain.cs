using OrleansPlusEventStoreDb.Demo.Models;

namespace OrleansPlusEventStoreDb.Demo.Grains;

[Alias("OrleansPlusEventStoreDb.Demo.Grains.IAccountBalanceGrain")]
public interface IAccountBalanceGrain : IGrainWithGuidKey
{
    [Alias("CreateAccount")]
    Task<ProcessingResult> CreateAccount(string currency, decimal initialBalance);
    
    [Alias("Deposit")]
    Task<ProcessingResult> Deposit(decimal amount);
    
    [Alias("Withdraw")]
    Task<ProcessingResult> Withdraw(decimal amount);

    [Alias("GetBalance")]
    Task<Balance> GetBalance();
}