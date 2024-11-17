using Balance_Support.DataClasses.DatabaseEntities;
using Balance_Support.DataClasses.Messages;
using Balance_Support.DataClasses.Records.NotificationData;
using Balance_Support.Scripts.Controllers.Interfaces;
using Balance_Support.Scripts.Database.Providers.Interfaces.Account;
using Balance_Support.Scripts.Database.Providers.Interfaces.Transaction;
using Balance_Support.Scripts.Database.Providers.Interfaces.User;
using Balance_Support.Scripts.Database.Providers.Interfaces.UserSettings;
using Balance_Support.Scripts.Main;
using Balance_Support.Scripts.Parsing;
using Balance_Support.Scripts.WebSockets.Interfaces;
using Newtonsoft.Json;

namespace Balance_Support.Scripts.Controllers;

public class TransactionController : ITransactionController
{
    private readonly IDatabaseAccountProvider accountProvider;
    private readonly INotificationMessageParser messageParser;
    private readonly IMessageSender messageSender;
    private readonly IDatabaseTransactionProvider transactionProvider;
    private readonly IDatabaseUserProvider userProvider;
    private readonly IDatabaseUserSettingProvider userSettingProvider;

    public TransactionController(INotificationMessageParser messageParser,
        IDatabaseAccountProvider accountProvider,
        IDatabaseTransactionProvider transactionProvider,
        IDatabaseUserSettingProvider userSettingProvider,
        IDatabaseUserProvider userProvider,
        IMessageSender messageSender)
    {
        this.messageParser = messageParser;
        this.accountProvider = accountProvider;
        this.transactionProvider = transactionProvider;
        this.userSettingProvider = userSettingProvider;
        this.userProvider = userProvider;
        this.messageSender = messageSender;
    }


    public async Task<IResult> RegisterNewTransaction(HttpContext context, NotificationHandleRequest handleRequest)
    {
        var data = await messageParser.HandleNotification(handleRequest);
        if (data == null) return Results.BadRequest("Incorrect notification text");

        var account = await accountProvider.GetAccountByUserIdAndBankCardNumber(handleRequest.UserId, data.CardNumber);
        if (account == null) return Results.NotFound("Account not found");

        var currentTransactions = (await transactionProvider.GetTransactionsByAccountId(account.Id)).ToList();
        var transaction = await transactionProvider.Register(handleRequest.UserId, account.Id, data.Type, data.Amount,
            data.Balance, handleRequest.NotificationText);

        // Update account balance based on transaction type
        if (!currentTransactions.Any())
            account.SmsBalance = transaction.Balance;
        else
            account.SmsBalance += transaction.Amount *
                                  (transaction.TransactionType == (int)TransactionType.Debiting ? -1 : 1);

        await accountProvider.UpdateAccount(account);

        // Retrieve MessagesGetRequest from session and deserialize it
        var messagesGetRequestJson = context.Session.GetString("MessagesGetRequest");
        MessagesGetRequest? messagesGetRequest = null;

        if (!string.IsNullOrEmpty(messagesGetRequestJson))
        {
            messagesGetRequest = JsonConvert.DeserializeObject<MessagesGetRequest>(messagesGetRequestJson);
        }

        // Check if transaction should be sent (if no filter or it matches the filter)
        var shouldSendTransactionMessage = messagesGetRequest == null ||
                                           messagesGetRequest.Matches(transaction, account.AccountNumber);

        MessageSendResult? resultTransactionMessage = null;
        if (shouldSendTransactionMessage)
        {
            // Send TransactionMessage if filter is null or matches
            resultTransactionMessage = await messageSender.SendMessage(handleRequest.UserId, new TransactionMessage
            {
                AccountId = account.Id,
                CardNumber = data.CardNumber,
                BankType = account.BankType,
                Channel = "sms",
                DeviceId = $"{account.AccountGroup},{account.DeviceId}",
                LastName = account.LastName,
                Incoming = data.Type == TransactionType.Debiting,
                Outgoing = data.Type == TransactionType.Crediting,
                SmsTime = $"{transaction.Time.TimeOfDay}",
                SmsDate = $"{transaction.Time:M/d/yyyy}",
                Message = handleRequest.NotificationText
            });
        }

        // Retrieve user settings for income message logic
        var settings = await userSettingProvider.GetByUserId(handleRequest.UserId);
        if (settings == null)
        {
            return Results.Created("Transaction/", new
            {
                Transaction = transaction.Convert(),
                TransactionResult = resultTransactionMessage,
                IncomeResult = "Cannot find user settings"
            });
        }

        if (settings.SelectedGroups.Any() && !settings.SelectedGroups.Contains(account.AccountGroup))
        {
            return Results.Created("Transaction/", new
            {
                Transaction = transaction.Convert(),
                transactionResult = resultTransactionMessage,
                IncomeResult = "Account not in selected group"
            });
        }

        // Calculate values and send income message if needed
        var income = await CalculateAccountIncome(handleRequest.UserId, settings.SelectedGroups);

        var curAccIncome = income.accsIncome.FirstOrDefault(x => x.Key.Id == account.Id).Value;

        if (curAccIncome == null)
        {
            return Results.Created("Transaction/", new
            {
                Transaction = transaction.Convert(),
                transactionResult = resultTransactionMessage,
                IncomeResult = "Not found account income"
            });
        }


        var resultIncomeMessage = await messageSender.SendMessage(handleRequest.UserId, new IncomeMessage
        {
            BalanceTotal = income.balance,
            DailyExpression = income.dailyExpression,
            AccountId = curAccIncome.AccId,
            Balance = curAccIncome.Balance,
            D = curAccIncome.Daily,
            T = curAccIncome.Total
        });

        return Results.Created("Transaction/", new
        {
            Transaction = transaction.Convert(),
            transactionResult = resultTransactionMessage,
            incomeResult = resultIncomeMessage
        });
    }


    public async Task<IResult> CalculateBalance(CalculateBalanceRequest request)
    {
        var userSettings = await userSettingProvider.GetByUserId(request.UserId);
        if (userSettings == null) return Results.NotFound("UserSettings");

        var valuesForSelectedGoup = await CalculateAccountIncome(request.UserId, userSettings.SelectedGroups);

        return Results.Ok(new
        {
            Balance = valuesForSelectedGoup.balance,
            DailyExplression = valuesForSelectedGoup.dailyExpression,
            AccountsIncome = valuesForSelectedGoup.accsIncome.Where(x => !x.Key.IsDeleted).Select(x => x.Value).ToList()
        });
    }


    public async Task<IResult> GetMessages(HttpContext httpContext, MessagesGetRequest messagesGetRequest)
    {
        // Save MessagesGetRequest to session as JSON
        var messagesGetRequestJson = JsonConvert.SerializeObject(messagesGetRequest);
        httpContext.Session.SetString("MessagesGetRequest", messagesGetRequestJson);

        // Your existing logic
        var accounts = new List<Account>();
        if (messagesGetRequest.AccountNumber != null)
        {
            var account =
                await accountProvider.GetAccountByUserIdAndAccountNumber(messagesGetRequest.UserId,
                    messagesGetRequest.AccountNumber);
            if (account != null) accounts.Add(account);
        }
        else
        {
            accounts = await accountProvider.FindAccountsByUserId(messagesGetRequest.UserId, true);
        }

        if (!accounts.Any())
            return Results.NotFound("Account");

        var messages = await transactionProvider.GetMessages(messagesGetRequest, accounts.Select(x => x.Id).ToList());

        var accountDict = accounts.ToDictionary(account => account.Id);

        var messageDtos = messages.Select(x => new TransactionMessage
        {
            AccountId = x.AccountId,
            CardNumber = accountDict[x.AccountId].BankCardNumber,
            BankType = accountDict[x.AccountId].BankType,
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


    private async Task<(decimal balance, decimal dailyExpression, Dictionary<Account, AccountIncomeData> accsIncome)>
        CalculateAccountIncome(string userId, List<int>? selectedGroups)
    {
        var accounts = await GetAccountWithTransactions(userId, selectedGroups);

        var accountsIncome = ConvertToAccountIncomeData(accounts);

        var balance = CalculateBalance(accountsIncome);

        var dailyExpression = CalculateDailyExpression(accountsIncome);
        return (balance, dailyExpression, accountsIncome);
    }

    private decimal CalculateBalance(Dictionary<Account, AccountIncomeData> incme)
        => incme.Where(x => !x.Key.IsDeleted).Sum(x => x.Value.Balance);

    private decimal CalculateDailyExpression(Dictionary<Account, AccountIncomeData> incme)
        => incme.Sum(x => x.Value.Daily);

    public Dictionary<Account, AccountIncomeData> ConvertToAccountIncomeData(
        Dictionary<Account, List<Transaction>> accountTransactions)
    {
        return accountTransactions.ToDictionary(
            kvp => kvp.Key, // Account as the key
            kvp =>
            {
                var account = kvp.Key;
                var transactions = kvp.Value;

                // Calculate total income
                var totalIncome = transactions
                    .Where(t => t.TransactionType == (int)TransactionType.Crediting)
                    .Sum(t => t.Amount) + account.InitialBalance;

                // Calculate daily income
                var dailyIncome = transactions
                    .Where(t => t.TransactionType == (int)TransactionType.Crediting &&
                                t.Time.Date == DateTime.UtcNow.Date)
                    .Sum(t => t.Amount);

                // Create AccountIncome
                return new AccountIncomeData
                {
                    AccId = account.Id,
                    Total = totalIncome,
                    Daily = dailyIncome,
                    Balance = account.SmsBalance
                };
            });
    }


    public async Task<Dictionary<Account, List<Transaction>>> GetAccountWithTransactions(
        string userId, List<int>? selectedGroup = null)
    {
        var accounts = await accountProvider.GetAccountsForUserSelectedGroupandIsDeleted(userId, selectedGroup, true);

        var accountTransactions = new Dictionary<Account, List<Transaction>>();

        foreach (var account in accounts)
        {
            var transactions = await transactionProvider.GetTransactionsByAccountId(account.Id);
            accountTransactions[account] = transactions;
        }

        return accountTransactions;
    }

}

public class AccountIncomeData
{
    public string AccId { get; set; }
    public decimal Total { get; set; }
    public decimal Daily { get; set; }

    public decimal Balance { get; set; }
}