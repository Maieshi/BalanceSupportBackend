using Balance_Support.DataClasses.DatabaseEntities;
using Balance_Support.DataClasses.Messages;
using Balance_Support.DataClasses.Records.NotificationData;
using Balance_Support.Scripts.Controllers.Interfaces;
using Balance_Support.Scripts.Database.Providers;
using Balance_Support.Scripts.Database.Providers.Interfaces;
using Balance_Support.Scripts.Database.Providers.Interfaces.Account;
using Balance_Support.Scripts.Database.Providers.Interfaces.Transaction;
using Balance_Support.Scripts.Database.Providers.Interfaces.UserSettings;
using Balance_Support.Scripts.Parsing;
using Balance_Support.Scripts.WebSockets.Interfaces;

namespace Balance_Support.Scripts.Controllers;

public class TransactionController : ITransactionController
{
    public async Task<IResult> RegisterNewTransaction(NotificationHandleRequest handleRequest,
        INotificationMessageParser messageParser, IGetAccountByUserIdAndBankCardNumber getUser,
        IRegisterTransaction transactionRegister, IMessageSender sender, IGetTransactionsForAccount getTransactions)
    {
        var data = await messageParser.HandleNotification(handleRequest);
        if (data == null)
        {
            return Results.BadRequest("Incorrect notification text");
        }

        var account = await getUser.GetAccountByUserIdAndBankCardNumber(handleRequest.UserId, data.CardNumber);

        if (account == null)
        {
            return Results.NotFound("Account not found");
        }

        var transaction = await transactionRegister.Register(handleRequest.UserId, account.Id, data.Type, data.Amount,
            data.Balance,
            handleRequest.NotificationText);

        var resultTransactionMessage = await sender.SendMessage(handleRequest.UserId, new TransactionMessage()
        {
            AccountId = account.Id,
            CardNumber = data.CardNumber,
            Channel = "sms",
            DeviceId = $"{account.AccountGroup},{account.DeviceId}",
            LastName = account.LastName,
            Incoming = data.Type == TransactionType.Debiting,
            Outgoing = data.Type == TransactionType.Crediting,
            SmsTime = $"{transaction.Time.TimeOfDay}",
            SmsDate = $"{transaction.Time:M/d/yyyy}",
            Message = handleRequest.NotificationText
        });

        var globalIncome = await CalculateIncomeForAccounts(new List<Account> { account }, getTransactions);

        var accIncome = globalIncome.Item2.First();

        var resultIncomeMessage = await sender.SendMessage(handleRequest.UserId, new IncomeMessage()
        {
            Balance = globalIncome.Item1.total,
            DailyExpression = globalIncome.Item1.daily,
            AccountId = accIncome.accId,
            T = accIncome.total,
            D = accIncome.daily
        });

        return Results.Created("Transaction/", new
        {
            Transaction = transaction,
            transactionResult = resultTransactionMessage,
            incomeResult = resultIncomeMessage
        });
    }

    public async Task<IResult> CalculateBalance(CalculateBalanceRequest request,
        IGetTransactionsForAccount getTransactions, IGetAccountsForUser getAccounts,
        IGetUserSettingsByUserId getUserSettings)
    {
        var userSettings = await getUserSettings.GetByUserId(request.UserId);
        if (userSettings == null)
        {
            return Results.NotFound("UserSettings");
        }

        var accounts = await getAccounts.GetAllAccountsForUser(request.UserId,
            userSettings.SelectedGroup == 0 ? null : userSettings.SelectedGroup);

        var incomeForAccounts = await CalculateIncomeForAccounts(accounts, getTransactions);


        return Results.Ok(new
        {
            Balance = incomeForAccounts.Item1.total,
            DailyExplression = incomeForAccounts.Item1.daily,
            AccountsIncome = incomeForAccounts.Item2
        });
    }

    public async Task<IResult> GetMessages(MessagesGetRequest messagesGetRequest,
        IGetMessages getMessages,
        IFindAccountByAccountId findAccount, IGetAccountByUserIdAndAccountNumber getAccount)
    {
        Account? account = null;
        if (messagesGetRequest.AccountNumber != null)
            account = await getAccount.GetAccountByUserIdAndAccountNumber(messagesGetRequest.UserId,
                messagesGetRequest.AccountNumber);

        var messages = await getMessages.GetMessages(messagesGetRequest, account?.Id);

        var accountDict =
            (await Task.WhenAll(messages
                    .Select(x => x.AccountId)
                    .Distinct()
                .Select(async x => await findAccount
                            .FindAccountByAccountId(x))))
            .Where(account => account != null)
            .ToDictionary(account => account!.Id);


        var messageDtos = messages.Select(x => new TransactionMessage()
        {
            AccountId = x.AccountId,
            CardNumber =accountDict[x.AccountId].BankCardNumber,
            Channel = "sms",
            DeviceId = $"{accountDict[x.AccountId].AccountGroup},{accountDict[x.AccountId].DeviceId}",
            LastName = accountDict[x.AccountId].LastName,
            Incoming = x.TransactionType == (int)TransactionType.Debiting,
            Outgoing = x.TransactionType == (int)TransactionType.Crediting,
            SmsTime = $"{x.Time.TimeOfDay}",
            SmsDate = $"{x.Time:M/d/yyyy}",
            Message = x.Message
        }).ToList();
        return Results.Ok(messageDtos);
    }

    private async Task<((float total, float daily), List<(string accId, float total, float daily)>)>
        CalculateIncomeForAccounts(List<Account> accounts, IGetTransactionsForAccount getTransactions)
    {
        var transactionsByAccount = (await Task.WhenAll(accounts.Select(async account =>
        {
            var transactions = await getTransactions.Get(account.Id);
            return new { AccountId = account.Id, Transactions = transactions };
        }))).ToDictionary(x => x.AccountId, x => x.Transactions);

        var incomePerAccount = transactionsByAccount.Select(x =>
        {
            var income = CalculateIncomeForTransactions(x.Value);
            return (accId: x.Key, total: income.total, daily: income.daily); // Use tuple instead of anonymous type
        }).ToList();

        var globalIncome = CalculateIncomeForTransactions(transactionsByAccount.SelectMany(kv => kv.Value).ToList());

        return (globalIncome, incomePerAccount);
    }


    private (float total, float daily) CalculateIncomeForTransactions(List<Transaction> transactions)
    {
        float totalIncome = (float)transactions.Sum(x => x.Amount);
        float dailyIncome = (float)transactions
            .Where(x => x.Time.Date == DateTime.UtcNow.Date)
            .Sum(x => x.Amount);
        return (totalIncome, dailyIncome);
    }
}