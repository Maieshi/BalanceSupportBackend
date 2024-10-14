using Balance_Support.DataClasses.DatabaseEntities;
using Balance_Support.DataClasses.Messages;
using Balance_Support.DataClasses.Records.NotificationData;
using Balance_Support.Scripts.Controllers.Interfaces;
using Balance_Support.Scripts.Database.Providers;
using Balance_Support.Scripts.Database.Providers.Interfaces;
using Balance_Support.Scripts.Database.Providers.Interfaces.Account;
using Balance_Support.Scripts.Database.Providers.Interfaces.Transaction;
using Balance_Support.Scripts.Database.Providers.Interfaces.UserSettings;
using Balance_Support.Scripts.Main;
using Balance_Support.Scripts.Parsing;
using Balance_Support.Scripts.WebSockets.Interfaces;
using Google.Apis.Util;

namespace Balance_Support.Scripts.Controllers;

public class TransactionController : ITransactionController
{
    public async Task<IResult> RegisterNewTransaction(NotificationHandleRequest handleRequest,
        INotificationMessageParser messageParser, IGetAccountByUserIdAndBankCardNumber getUser,
        IRegisterTransaction transactionRegister, IMessageSender sender,
        IGetTransactionsForAccount getTransactions,IGetAccountsForUser getAccounts,
        IGetUserSettingsByUserId  getUserSettings)
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

        var settings = await getUserSettings.GetByUserId(handleRequest.UserId);
        
        if(settings==null)
            return Results.Created("Transaction/", new
            {
                Transaction = new TransactionDto(transaction),
                transactionResult = resultTransactionMessage,
                incomeResult = "Cannot find user settings"
            });

        var groupIncome = await CalculateTotalIncomeForSelectedGroup(handleRequest.UserId,settings.SelectedGroup,getTransactions,getAccounts);
        
        var accIncome = CalculateIncomeForTransactions(await getTransactions.Get(account.Id));

        var resultIncomeMessage = await sender.SendMessage(handleRequest.UserId, new IncomeMessage()
        {
            Balance = groupIncome.total,
            DailyExpression = groupIncome.daily,
            AccountId = account.Id,
            T = accIncome.total+(float)account.InitialBalance,
            D = accIncome.daily
        });

        return Results.Created("Transaction/", new
        {
            Transaction = new TransactionDto(transaction),
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

        var totalIncomeForSelectedGroup = await CalculateTotalIncomeForSelectedGroup(request.UserId, userSettings.SelectedGroup,
            getTransactions, getAccounts);

        var incomeForSelectedGroup = await CalculateAccountIncomeForSelectedGroup(request.UserId, userSettings.SelectedGroup,
            getTransactions, getAccounts);

        return Results.Ok(new
        {
            Balance = totalIncomeForSelectedGroup.total,
            DailyExplression = totalIncomeForSelectedGroup.daily,
            AccountsIncome = incomeForSelectedGroup
        });
    }

    public async Task<IResult> GetMessages(MessagesGetRequest messagesGetRequest,
        IGetMessages getMessages,
        IFindAccountByAccountId findAccount, IGetAccountByUserIdAndAccountNumber getAccount,IFindAccountsByUserId findAccounts)
    {
        List<Account> accounts = new List<Account>();
        if (messagesGetRequest.AccountNumber != null)
        {
            var account =
                await getAccount.GetAccountByUserIdAndAccountNumber(messagesGetRequest.UserId,
                    messagesGetRequest.AccountNumber);
            if (account != null)
            {
                accounts.Add(account);
            }
        }
        else
            accounts = await findAccounts.FindAccountsByUserId(messagesGetRequest.UserId);

        if (!accounts.Any())
            return Results.NotFound("Account");
        
        var messages = await getMessages.GetMessages(messagesGetRequest,accounts.Select(x=> x.Id).ToList());

        var accountDict =
            accounts.ToDictionary(account => account.Id);

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

    private async Task<(float total, float daily)> CalculateTotalIncomeForSelectedGroup(string userId,int selectedGroup,IGetTransactionsForAccount getTransactions, IGetAccountsForUser getAccounts)
    {
        var accounts = (await getAccounts.GetAllAccountsForUser(userId,
            selectedGroup == 0 ? null : selectedGroup)).ToDictionary(x=>x.Id);
        
        var transactionsByAccount = new Dictionary<string, List<Transaction>>();

        foreach (var account in accounts)
        {
            var transactions = await getTransactions.Get(account.Key);
            transactionsByAccount[account.Key] = transactions;
        }

        var incomePerAccount = transactionsByAccount.Select(x =>
        {
            var income = CalculateIncomeForTransactions(x.Value);
            return (total: income.total+(float)accounts[x.Key].InitialBalance ,daily: income.daily);
        }).ToList();

        var total = incomePerAccount.Sum(x => x.total);
        
        var daily = incomePerAccount.Sum(x => x.daily);

        return (total,daily);
    }
    
    private async Task<List<AccountIncome>> CalculateAccountIncomeForSelectedGroup(string userId,int selectedGroup,IGetTransactionsForAccount getTransactions, IGetAccountsForUser getAccounts)
    {
        var accounts = (await getAccounts.GetAllAccountsForUser(userId,
            selectedGroup == 0 ? null : selectedGroup)).ToDictionary(x=>x.Id);
        
        var transactionsByAccount = new Dictionary<string, List<Transaction>>();

        foreach (var account in accounts)
        {
            var transactions = await getTransactions.Get(account.Key);
            transactionsByAccount[account.Key] = transactions;
        }

        return transactionsByAccount.Select(x =>
        {
            var income = CalculateIncomeForTransactions(x.Value);
            return new AccountIncome(){ AccId = x.Key,Total = income.total+(float)accounts[x.Key].InitialBalance , Daily = income.daily};
        }).ToList();
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

public class AccountIncome
{
    public string AccId { get; set; }
    public float Total { get; set; }
    public float Daily { get; set; }
}