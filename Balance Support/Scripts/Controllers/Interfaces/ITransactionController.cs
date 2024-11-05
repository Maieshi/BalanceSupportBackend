using Balance_Support.DataClasses.Records.NotificationData;
using Balance_Support.Scripts.Database.Providers.Interfaces;
using Balance_Support.Scripts.Database.Providers.Interfaces.Account;
using Balance_Support.Scripts.Database.Providers.Interfaces.Transaction;
using Balance_Support.Scripts.Database.Providers.Interfaces.User;
using Balance_Support.Scripts.Database.Providers.Interfaces.UserSettings;
using Balance_Support.Scripts.Parsing;
using Balance_Support.Scripts.WebSockets.Interfaces;

namespace Balance_Support.Scripts.Controllers.Interfaces;

public interface ITransactionController
{
    public Task<IResult> RegisterNewTransaction(NotificationHandleRequest handleRequest);

    public Task<IResult> CalculateBalance(CalculateBalanceRequest request);

    public Task<IResult> GetMessages(MessagesGetRequest messagesGetRequest);
}