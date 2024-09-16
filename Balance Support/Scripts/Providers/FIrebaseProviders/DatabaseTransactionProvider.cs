// using FireSharp.Interfaces;
// using FireSharp.Response;
// using FireSharp.Config;

using Balance_Support.DataClasses;
using Balance_Support.DataClasses.DatabaseEntities;
using Balance_Support.Interfaces;
using FirebaseAdmin.Messaging;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
// using Google.Apis.Auth.OAuth2;

namespace Balance_Support;

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
            return Results.Problem(statusCode: 500, title: "Account not found");


        var transactionData = new Transaction
        {
            Id = Guid.NewGuid().ToString(),
            TransactionType = (int)transactionType,
            Amount = amount,
            Balance = balance,
            Message = message,
            AccountId = account.Id,
            Time = DateTime.Now
        };

        try
        {
            context.Transactions.Add(transactionData);
            await context.SaveChangesAsync();
            var result = cloudMessagingProvider.SendMessage(userId, account, transactionData);
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

        return Results.Created("Transactions", JsonConvert.SerializeObject(transactionData));
    }

    public async Task<IResult> GetTransactionsForUser(string userId, int amount)
    {
        var transactions = context.Transactions.Where(x => x.UserId == userId);

        var distinctAccountIds = transactions.Select(transaction => transaction.AccountId).Distinct().ToList();
        
        var accounts = await Task.WhenAll(distinctAccountIds.Select(id => context.Accounts.FirstOrDefaultAsync(a => a.Id == id)));
        foreach (var transaction in transactions)
        {
            var account = accounts.FirstOrDefault(x=>x.Id == transaction.AccountId);
            if(account!=null)
            await cloudMessagingProvider.SendMessage(userId,account, transaction);
        }
        return Results.Ok();
    }
}