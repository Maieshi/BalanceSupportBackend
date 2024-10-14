using Balance_Support.DataClasses.Records.NotificationData;
using Balance_Support.Scripts.Database.Providers.Interfaces;
using Balance_Support.Scripts.Database.Providers.Interfaces.Account;
using Balance_Support.Scripts.Database.Providers.Interfaces.Transaction;
using Balance_Support.Scripts.Database.Providers.Interfaces.UserSettings;
using Balance_Support.Scripts.Parsing;
using Balance_Support.Scripts.WebSockets.Interfaces;

namespace Balance_Support.Scripts.Controllers.Interfaces;

public interface ITransactionController
{
    public Task<IResult> RegisterNewTransaction(NotificationHandleRequest handleRequest,
        INotificationMessageParser messageParser, IGetAccountByUserIdAndBankCardNumber getUser,
        IRegisterTransaction transactionRegister, IMessageSender sender,
        IGetTransactionsForAccount getTransactions, IGetAccountsForUser getAccounts,
        IGetUserSettingsByUserId getUserSettings);

    public Task<IResult> CalculateBalance(CalculateBalanceRequest request,
        IGetTransactionsForAccount getTransactions, IGetAccountsForUser getAccounts,
        IGetUserSettingsByUserId getUserSettings);

    public Task<IResult> GetMessages(MessagesGetRequest messagesGetRequest,
        IGetMessages getMessages,
        IFindAccountByAccountId findAccount, IGetAccountByUserIdAndAccountNumber getAccount,IFindAccountsByUserId findAccounts);
}