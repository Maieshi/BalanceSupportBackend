using Balance_Support.DataClasses.Records.NotificationData;
using Balance_Support.Scripts.Parsing;

namespace Balance_Support.Scripts.Database.Providers.Interfaces.Transaction;

public interface IDatabaseTransactionProvider
{
    public Task<List<DataClasses.DatabaseEntities.Transaction>> GetMessages(MessagesGetRequest messagesGetRequest, List<string> accountIds);
    public Task<List<DataClasses.DatabaseEntities.Transaction>> GetByAccountId(string accountId);
    public Task<DataClasses.DatabaseEntities.Transaction> Register(
        string userId,
        string accountId,
        TransactionType transactionType,
        decimal amount,
        decimal balance,
        string message
    );    
}