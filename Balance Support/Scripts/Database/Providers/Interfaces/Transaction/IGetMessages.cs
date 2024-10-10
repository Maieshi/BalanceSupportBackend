using Balance_Support.DataClasses.Records.NotificationData;

namespace Balance_Support.Scripts.Database.Providers.Interfaces.Transaction;

public interface IGetMessages
{
    public Task<List<DataClasses.DatabaseEntities.Transaction>> GetMessages(MessagesGetRequest messagesGetRequest, string? accountId = null);
}