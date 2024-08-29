
// using FireSharp;
// using FireSharp.Interfaces;
// using FireSharp.Response;
// using FireSharp.Config;

using System.IO.Enumeration;
using Firebase.Database;
using Firebase.Database.Query;
// using Google.Apis.Auth.OAuth2;
using Newtonsoft.Json;
using Balance_Support.Interfaces;
using Balance_Support.Scripts.Extensions;
using Balance_Support.SerializationClasses;
using Balance_Support.DataClasses.Records.AccountData;
using Balance_Support.DataClasses.Records.NotificationData;
using System.Text.RegularExpressions;
using Balance_Support.DataClasses.Records.NotificationData.DatabaseInfo;

namespace Balance_Support;

public class NotificationHandler
{
    private readonly DatabaseAccountProvider provider;
    private readonly FirebaseClient client;

    public NotificationHandler(DatabaseAccountProvider provider, FirebaseClient client)
    {
        this.provider = provider;
        this.client = client;
    }
    
    public async Task<IResult> RegisterNotificationData(PutNotificationRequest request)
    {
        var cardNumberMatch = Regex.Match(request.NotificationText, @"\b(?:MIR-|СЧЁТ|)(\d{4})\b");
        
        if (!cardNumberMatch.Success||int.TryParse(cardNumberMatch.Groups[1].Value, out var cardLastFourDigits))
        {
            return Results.Problem(statusCode: 500, title: "Incorrect notification text");
        }
          
        var transactionMatch = Regex.Match(request.NotificationText, @"(?:(зачисление|Перевод из|перевод)\s+([\d.,]+)(?:р|\+))");

        if (!transactionMatch.Success || !decimal.TryParse(
                transactionMatch.Groups[2].Value.Replace(",", "."), 
                System.Globalization.NumberStyles.Number, 
                System.Globalization.CultureInfo.InvariantCulture, 
                out var amount))
        {
            return Results.Problem(statusCode: 500, title: "Incorrect notification text");
        }

        TransactionType transactionType = transactionMatch.Groups[1].Value switch
        {
            "зачисление" or "Перевод из" => TransactionType.Crediting,
            "перевод" => TransactionType.Debiting,
            _ => throw new ArgumentOutOfRangeException()
        };
        
        var balanceMatch = Regex.Match(request.NotificationText, @"Баланс:\s*([\d\s]+(?:,\d{1,2})?)р");
        
        decimal balance = 0;

        if (!balanceMatch.Success && !decimal.TryParse(
                balanceMatch.Groups[1].Value.Replace(" ", ""), 
                System.Globalization.NumberStyles.Number, 
                System.Globalization.CultureInfo.InvariantCulture, 
                out  balance))
        {
            return Results.Problem(statusCode: 500, title: "Incorrect notification text");
        }
        
        var account = await provider.FindAccountByAccountId(request.AccountId);
        
        if(account == default)
            return Results.Problem(statusCode: 500, title: "Account not found");
        
         var transaction = await client
             .Child("Transactions")
             .PostAsync(new TransactionData
                 (account.Object.AccountId, transactionType, amount,  balance, request.NotificationText));
       
       
        
       return Results.Ok(transaction.Object);
    }
}

public enum TransactionType
{
    Crediting,
    Debiting
}

