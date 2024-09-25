
// using FireSharp.Interfaces;
// using FireSharp.Response;
// using FireSharp.Config;

using Balance_Support.DataClasses.DatabaseEntities;
using Balance_Support.DataClasses.Records.NotificationData;
using Balance_Support.Scripts.Providers.Interfaces;
using FirebaseAdmin.Messaging;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

// using Google.Apis.Auth.OAuth2;

namespace Balance_Support.Scripts.Providers;

public class DatabaseTransactionProvider : IDatabaseTransactionProvider
{
    private readonly ICloudMessagingProvider cloudMessagingProvider;
    private readonly ApplicationDbContext context;
    private readonly IDatabaseAccountProvider accountProvider;

    public DatabaseTransactionProvider(IDatabaseAccountProvider accountProvider,
        ICloudMessagingProvider cloudMessagingProvider, ApplicationDbContext context)
    {
        this.accountProvider = accountProvider;
        this.cloudMessagingProvider = cloudMessagingProvider;
        this.context = context;
    }

    public async Task<IResult> RegisterNewTransaction(
        string userId,
        TransactionType transactionType,
        string cardNumber,
        decimal amount,
        decimal balance,
        string message
    )
    {
        var account = await accountProvider.GetAccountByUserIdAndBankCardNumber(userId, cardNumber);

        if (account == default)
            return Results.NotFound("Account");
        var newId = Guid.NewGuid().ToString();

        var transactionData = new Transaction
        {
            Id = newId,
            AccountId = account.Id,
            UserId = userId,
            TransactionType = (int)transactionType,
            Amount = amount,
            Balance = balance,
            Message = message,
            Time = DateTime.UtcNow
        };

        try
        {
            context.Transactions.Add(transactionData);
            await context.SaveChangesAsync();
            var result = cloudMessagingProvider.SendTransaction(userId, account, transactionData);
        }
        catch (FirebaseMessagingException e)
        {
            return Results.Problem(statusCode: 500, title: "Cannot send message to user");
        }
        catch (ArgumentNullException e)
        {
            return Results.Problem(statusCode: 500, title: "Cannot send message to user");
        }
        catch (ArgumentException e)
        {
            return Results.Problem(statusCode: 500, title: "Cannot send message to user");
        }
        catch (DbUpdateException e)
        {
            return Results.Problem(statusCode: 500, title: "Cannot put transaction to database");
        }
        catch (OperationCanceledException e)
        {
            return Results.Problem(statusCode: 500, title: "Cannot put transaction to database");
        }

        return Results.Created("Transactions", newId);
    }

    public async Task<IResult> GetTransactionsForUser(TransactionGetRequest transactionGetRequest)
    {
        var transactions =await context.Transactions.Where(x => x.UserId == transactionGetRequest.UserId).ToListAsync();

        var distinctAccountIds = transactions.Select(transaction => transaction.AccountId).Distinct().ToList();

        var accounts =
            await Task.WhenAll(distinctAccountIds.Select(id => context.Accounts.FirstOrDefaultAsync(a => a.Id == id)));
        foreach (var transaction in transactions)
        {
            var account = accounts.FirstOrDefault(x => x.Id == transaction.AccountId);
            if (account != null)
                await cloudMessagingProvider.SendTransaction(transactionGetRequest.UserId, account, transaction);
        }

        return Results.Ok();
    }

    public async Task<IResult> GetMessages(MessagesGetRequest messagesGetRequest)
    {
        var query = context.Transactions.AsQueryable();
        query = query.Where(x => x.UserId == messagesGetRequest.UserId);
        
        Account? filteredAccount;
        if (!string.IsNullOrEmpty(messagesGetRequest.AccountNumber))
        {
            filteredAccount =
                await accountProvider.GetAccountByUserIdAndAccountNumber(messagesGetRequest.UserId,
                    messagesGetRequest.AccountNumber);
            if (filteredAccount != null)
            {
                query = query.Where(t => t.AccountId == filteredAccount.Id);
            }
            else return Results.NotFound("Account with this account number");
        }

        if (!string.IsNullOrEmpty(messagesGetRequest.SearchText))
        {
            query = query.Where(t => t.Message.Contains(messagesGetRequest.SearchText));
        }

        if (messagesGetRequest.StartingDate.HasValue)
        {
            query = query.Where(t => t.Time >= messagesGetRequest.StartingDate.Value);
        }

        if (messagesGetRequest.EndingDate.HasValue)
        {
            query = query.Where(t => t.Time <= messagesGetRequest.EndingDate.Value);
        }

        if (messagesGetRequest.MessageType.HasValue&&messagesGetRequest.MessageType.Value!=-1)
        {
            query.Where(t => t.TransactionType == messagesGetRequest.MessageType.Value);
        }


        var transactions = await query
            .Take(messagesGetRequest.Amount)
            .ToListAsync();

        var accountIds = transactions.Select(t => t.AccountId).Distinct();

        var accounts = await context.Accounts
            .Where(a => accountIds.Contains(a.Id))
            .ToListAsync();
        if (transactions.Any(transaction => !accounts.Any(account => account.Id == transaction.AccountId))) 
            return Results.NotFound("Account with the same Id as the transaction");
        return await cloudMessagingProvider.SendMessages(messagesGetRequest.UserId, transactions, accounts);
    }
}