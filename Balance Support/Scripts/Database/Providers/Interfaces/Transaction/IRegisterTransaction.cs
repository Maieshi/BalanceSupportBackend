using Balance_Support.Scripts.Parsing;

namespace Balance_Support.Scripts.Database.Providers.Interfaces.Transaction;

public interface IRegisterTransaction
{
    public Task<DataClasses.DatabaseEntities.Transaction> Register(
        string userId,
        string accountId,
        TransactionType transactionType,
        decimal amount,
        decimal balance,
        string message
    );
}