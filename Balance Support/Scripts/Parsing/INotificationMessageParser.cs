using Balance_Support.DataClasses.Records.NotificationData;

namespace Balance_Support.Scripts.Parsing;

public interface INotificationMessageParser
{
    public Task<TransactionParsedData?> HandleNotification(NotificationHandleRequest request);

    // public void Test();
}