namespace OrleansPlusEventStoreDb.Demo.Models;

public enum TransactionType
{
    Withdrawal,
    Deposit
}

public record Transaction(TransactionType TransactionType, decimal Amount, decimal Currency);