// using FireSharp.Interfaces;
// using FireSharp.Response;
// using FireSharp.Config;
using Firebase.Database;
using Firebase.Database.Query;
// using Google.Apis.Auth.OAuth2;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using System.Linq;
using Balance_Support.Interfaces;
using Balance_Support.Scripts.Extensions;
using Balance_Support.SerializationClasses;
using Balance_Support.DataClasses.Records.AccountData;
using Balance_Support.DataClasses.Records.NotificationData.DatabaseInfo;
using Balance_Support.Scripts.Extensions.RecordExtenstions;

namespace Balance_Support;

public class DatabaseTransactionProvider:IDatabaseTransactionProvider
{
    private readonly FirebaseClient client;
    private readonly IDatabaseAccountProvider provider;
    private readonly ICloudMessagingProvider cloudMessagingProvider;

    public DatabaseTransactionProvider(FirebaseClient client, IDatabaseAccountProvider provider, ICloudMessagingProvider cloudMessagingProvider)
    {
        this.client = client;
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
        var bank = await provider.GetAccountByUserIdAndBankCardNumber(userId,cardNumber);
        
        if (bank == default)
            return Results.Problem(statusCode: 500, title: "Account not found");


        var transactionData = new TransactionData
            (bank.Object.AccountId, transactionType, amount, balance, DateTime.Now, message);
        
        var transaction = await client
            .Child("Transactions")
            .PostAsync(transactionData);


        try
        {
            var result = cloudMessagingProvider.SendMessage(userId, bank.Object, transactionData);
        }
        catch (Exception e)
        {
            return Results.Problem(statusCode: 500, title: "Cannot send message to user");
        }
        
        return Results.Created($"Transactions", JsonConvert.SerializeObject(transaction));
    }
}
