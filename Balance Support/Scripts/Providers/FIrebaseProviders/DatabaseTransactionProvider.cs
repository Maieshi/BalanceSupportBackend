// using FireSharp.Interfaces;
// using FireSharp.Response;
// using FireSharp.Config;
using Firebase.Database;
using Firebase.Database.Query;
// using Google.Apis.Auth.OAuth2;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using System.Linq;
using Balance_Support.DataClasses;
using Balance_Support.Interfaces;
using Balance_Support.Scripts.Extensions;
using Balance_Support.SerializationClasses;
using Balance_Support.DataClasses.Records.AccountData;
using Balance_Support.DataClasses.Records.NotificationData.DatabaseInfo;
using Balance_Support.Scripts.Extensions.RecordExtenstions;
using Balance_Support.DataClasses.DatabaseEntities;
namespace Balance_Support;

public class DatabaseTransactionProvider:IDatabaseTransactionProvider
{
    private readonly IDatabaseAccountProvider provider;
    private readonly ICloudMessagingProvider cloudMessagingProvider;

    public DatabaseTransactionProvider(IDatabaseAccountProvider provider, ICloudMessagingProvider cloudMessagingProvider)
    {
        this.provider = provider;
        this.cloudMessagingProvider = cloudMessagingProvider;
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
        var account = await provider.GetAccountByUserIdAndBankCardNumber(userId,cardNumber);
        
        if (account == default)
            return Results.Problem(statusCode: 500, title: "Account not found");



        
        var transactionData = new Transaction()
        {
            TransactionType = (int)transactionType,
            Amount = amount,
            Balance = balance,
            Message = message
        };
        
            
        
        

        try
        {
            var result = cloudMessagingProvider.SendMessage(userId, bank.Object, transactionData);
        }
        catch (Exception e)
        {
            return Results.Problem(statusCode: 500, title: "Cannot send message to user");
        }
        
        return Results.Created($"Transactions", JsonConvert.SerializeObject(transactionData));
    }
    
    public async Task<IResult> GetTransactionsForUser(string userId, int amount)
    {
        var transactions = (await client
                .Child("Transactions")
                .OrderBy("UserId")
                .EqualTo(userId)
                .LimitToFirst(amount)
                .OnceAsync<TransactionData>())
            .Where(device => device != null);
        
        var distinctAccountIds = transactions.Select(transaction => transaction.Object.AccountId).Distinct().ToList();
        
        var accounts = (await Task.WhenAll(
            distinctAccountIds.Select(id => accountProvider.FindAccountByAccountId(id))
        )).Where(x=>x!=null).Cast<FirebaseObject<AccountData>>();
            // Explicitly cast to non-nullable type
            foreach (var transaction in transactions)
            {
                var account = accounts.FirstOrDefault(x=>x.Object.AccountId == transaction.Object.AccountId);
                if(account!=null)
                await cloudMessagingProvider.SendMessage(userId,account.Object, transaction.Object);
            }
        return Results.Ok();
    }
}
