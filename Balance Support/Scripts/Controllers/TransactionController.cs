using Balance_Support.DataClasses.DatabaseEntities;
using Balance_Support.DataClasses.Messages;
using Balance_Support.DataClasses.Records.NotificationData;
using Balance_Support.Scripts.Controllers.Interfaces;
using Balance_Support.Scripts.Database;
using Balance_Support.Scripts.Database.Providers.Interfaces.Account;
using Balance_Support.Scripts.Database.Providers.Interfaces.Transaction;
using Balance_Support.Scripts.Database.Providers.Interfaces.UserSettings;
using Balance_Support.Scripts.Main;
using Balance_Support.Scripts.Parsing;
using Balance_Support.Scripts.WebSockets.Interfaces;

namespace Balance_Support.Scripts.Controllers;

public class TransactionController : ITransactionController
{
    public async Task<IResult> RegisterNewTransaction(NotificationHandleRequest handleRequest,
        INotificationMessageParser messageParser, IGetAccountByUserIdAndBankCardNumber getUser,
        IRegisterTransaction transactionRegister, IMessageSender sender,
        IGetTransactionsForAccount getTransactions, IGetAccountsForUser getAccounts,
        IGetUserSettingsByUserId getUserSettings, IUpdateAccount updateAccount)
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

        var currentTransactions = (await getTransactions.Get(handleRequest.UserId)).ToList();
        var transaction = await transactionRegister.Register(handleRequest.UserId, account.Id, data.Type, data.Amount,
            data.Balance,
            handleRequest.NotificationText);


        if (!currentTransactions.Any())
        {
            account.SmsBalance = transaction.Balance;
            await updateAccount.UpdateAccount(account);
        }

        var resultTransactionMessage = await sender.SendMessage(handleRequest.UserId, new TransactionMessage()
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

        var settings = await getUserSettings.GetByUserId(handleRequest.UserId);

        if (settings == null)
            return Results.Created("Transaction/", new
            {
                Transaction = transaction.Convert(),
                TransactionResult = resultTransactionMessage,
                IncomeResult = "Cannot find user settings"
            });

        if (account.AccountGroup != settings.SelectedGroup && settings.SelectedGroup != 0)
            return Results.Created("Transaction/", new
            {
                Transaction = transaction.Convert(),
                transactionResult = resultTransactionMessage,
                IncomeResult = "Account not in selected group"
            });

        var selectedGroupValues = await CalculateValuesForSelectedGroup(handleRequest.UserId, settings.SelectedGroup,
            getTransactions, getAccounts);

        var balance = CalculateBalance(selectedGroupValues);

        var dailyExplession = CalculateDailyExplession(selectedGroupValues);

        var resultIncomeMessage = await sender.SendMessage(handleRequest.UserId, new IncomeMessage()
        {
            BalanceTotal = balance,
            DailyExpression = dailyExplession,
            AccountId= account.Id,
            BalanceAccount = selectedGroupValues[account].balance,
            D =  selectedGroupValues[account].daily,
            T =  selectedGroupValues[account].total
        });

        return Results.Created("Transaction/", new
        {
            Transaction = transaction.Convert(),
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

        var valuesForSelectedGoup = await CalculateValuesForSelectedGroup(request.UserId, userSettings.SelectedGroup,
            getTransactions, getAccounts);

        return Results.Ok(new
        {
            Balance = CalculateBalance(valuesForSelectedGoup),
            DailyExplression = CalculateDailyExplession(valuesForSelectedGoup),
            AccountsIncome = ConvertToAccountsIncome(valuesForSelectedGoup)
        });
    }

    public async Task<IResult> GetMessages(MessagesGetRequest messagesGetRequest,
        IGetMessages getMessages,
        IFindAccountByAccountId findAccount, IGetAccountByUserIdAndAccountNumber getAccount,
        IFindAccountsByUserId findAccounts)
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

        var messages = await getMessages.GetMessages(messagesGetRequest, accounts.Select(x => x.Id).ToList());

        var accountDict =
            accounts.ToDictionary(account => account.Id);

        var messageDtos = messages.Select(x => new TransactionMessage()
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

    private async Task<Dictionary<Account, (decimal balance, decimal total, decimal daily)>>
        CalculateValuesForSelectedGroup(string userId, int selectedGroup,
            IGetTransactionsForAccount getTransactions, IGetAccountsForUser getAccounts)
    {
        var accounts = await getAccounts.GetAllAccountsForUser(userId,
            selectedGroup == 0 ? null : selectedGroup);


        return await CalculateValuesForAccounts(accounts, getTransactions);
    }

    private async Task<Dictionary<Account, (decimal balance, decimal total, decimal daily)>> CalculateValuesForAccounts(
        List<Account> accounts, IGetTransactionsForAccount getTransactions)
    {
        var transactionsByAccount = await GetTransactionsForAccounts(accounts, getTransactions);

        return (await Task.WhenAll(transactionsByAccount.Select(async kvp =>
                new { kvp.Key, Values = AddBalances(await CalculateValuesForTransactions(kvp.Value), kvp.Key) })))
            .ToDictionary(x => x.Key, x => x.Values);
    }

    private async Task<(decimal balance, decimal total, decimal daily)> CalculateValuesForTransactions(
        List<Transaction> transactions)
    {
        var balance = CalculateBalance(transactions);

        var total = CalculateIncome(transactions);

        var daily = CalculateIncomeForToday(transactions);

        return (balance, total, daily);
    }

    private (decimal balance, decimal total, decimal daily) AddBalances(
        (decimal balance, decimal total, decimal daily) data, Account account)
    {
        return (data.balance + account.SmsBalance, data.total + account.InitialBalance, data.daily);
    }

    private List<AccountIncome> ConvertToAccountsIncome(
        Dictionary<Account, (decimal balance, decimal total, decimal daily)> accountValues)
    {
        return accountValues.Select(x =>
            new AccountIncome()
            {
                AccId = x.Key.Id,
                Total = x.Value.total,
                Daily = x.Value.daily
            }).ToList();
    }

    private decimal CalculateDailyExplession(
        Dictionary<Account, (decimal balance, decimal total, decimal daily)> valuesForAcounts)
    {
        return valuesForAcounts.Values.Sum(x => x.daily);
    }

    private decimal CalculateBalance(
        Dictionary<Account, (decimal balance, decimal total, decimal daily)> valuesForAcounts)
    {
        return valuesForAcounts.Values.Sum(x => x.balance);
    }

    private decimal CalculateBalance(List<Transaction> transactions, decimal smsBalance)
    {
        return transactions.Sum(x =>
            x.Amount * (x.TransactionType == (int)TransactionType.Crediting ? -1 : 1) + smsBalance);
    }

    private decimal CalculateBalance(List<Transaction> transactions)
    {
        return transactions.Sum(x => x.Amount * (x.TransactionType == (int)TransactionType.Crediting ? -1 : 1));
    }

    private decimal CalculateIncome(List<Transaction> transactions)
    {
        return transactions.Where(x => x.TransactionType == (int)TransactionType.Debiting).Sum(x => x.Amount);
    }

    private decimal CalculateIncomeForToday(List<Transaction> transactions)
    {
        return transactions.Where(x =>
                x.TransactionType == (int)TransactionType.Debiting && x.Time.Date == ConstStorage.MoscowUtcNow.Date)
            .Sum(x => x.Amount);
    }

    private async Task<Dictionary<Account, List<Transaction>>> GetTransactionsForAccounts(
        Dictionary<string, Account> accounts, IGetTransactionsForAccount getTransactions)
    {
        return (await Task.WhenAll(
            accounts.Select(async account => new
            {
                Key = account.Value,
                Transactions = await getTransactions.Get(account.Key)
            })
        )).ToDictionary(x => x.Key, x => x.Transactions);
    }

    private async Task<Dictionary<Account, List<Transaction>>> GetTransactionsForAccounts(
        List<Account> accounts,
        IGetTransactionsForAccount getTransactions)
    {
        var result = new Dictionary<Account, List<Transaction>>();
        foreach (var account in accounts)
        {
            var transactions = await getTransactions.Get(account.Id);
            result[account] = transactions;
        }

        return result;
    }

}

public class AccountIncome
{
    public string AccId { get; set; }
    public decimal Total { get; set; }
    public decimal Daily { get; set; }

    public decimal Balance { get; set; }
}