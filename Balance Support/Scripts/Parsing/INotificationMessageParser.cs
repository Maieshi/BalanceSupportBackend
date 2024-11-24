using Balance_Support.DataClasses.DatabaseEntities;
using Balance_Support.DataClasses.Records.NotificationData;

namespace Balance_Support.Scripts.Parsing;

public interface INotificationMessageParser
{
    public Task<TransactionParsedData?> ParseMessage(NotificationHandleRequest request);
    public Task<TransactionParsedData?> ParseSimpleMessage(Account account, string message);

    // public void Test();
}