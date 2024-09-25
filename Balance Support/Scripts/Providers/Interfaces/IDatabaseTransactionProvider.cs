using Balance_Support.DataClasses.Records.NotificationData;

namespace Balance_Support.Scripts.Providers.Interfaces;

public interface IDatabaseTransactionProvider
{
    public Task<IResult> RegisterNewTransaction(
        string userId,
        TransactionType transactionType,
        string cardNumber,
        decimal amount,
        decimal balance,
        string message
    );

    public Task<IResult> GetTransactionsForUser(TransactionGetRequest transactionGetRequest);

    public Task<IResult> GetMessages(MessagesGetRequest messagesGetRequest);
}