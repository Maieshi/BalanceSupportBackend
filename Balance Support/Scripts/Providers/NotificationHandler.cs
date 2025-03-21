using System.Text.RegularExpressions;
using Balance_Support.DataClasses.Records.NotificationData;
using Balance_Support.Scripts.Providers.Interfaces;
using Firebase.Database;

namespace Balance_Support.Scripts.Providers;

public class NotificationHandler : INotificationHandler
{
    private readonly FirebaseClient client;
    private readonly IDatabaseTransactionProvider transactionProvider;

    public NotificationHandler(FirebaseClient client, IDatabaseTransactionProvider transactionProvider)
    {
        this.client = client;
        this.transactionProvider = transactionProvider;
    }

    public async Task<IResult> HandleNotification(NotificationHandleRequest request)
    {
        var cardNumberMatch = Regex.Match(request.NotificationText, @"\b(?:MIR-|СЧЁТ|)(\d{4})\b");

        if (!cardNumberMatch.Success)
        {
            return Results.Problem(statusCode: 500, title: "Incorrect notification text");
        }

        string cardLastFourDigits = cardNumberMatch.Groups[1].Value;

        var transactionMatch = Regex.Match(request.NotificationText,
            @"(?:(зачисление|Перевод из|перевод)\s+([\d.,]+)(?:р|\+))");

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

        var balanceMatch = Regex.Match(request.NotificationText, @"Баланс:\s*([\d\s]+(?:,\d{1,2})?\.\d{1,2})р");
        decimal balance = 0;

        if (!balanceMatch.Success && !decimal.TryParse(
                balanceMatch.Groups[1].Value.Replace(" ", ""),
                System.Globalization.NumberStyles.Number,
                System.Globalization.CultureInfo.InvariantCulture,
                out balance))
        {
            return Results.Problem(statusCode: 500, title: "Incorrect notification text");
        }

        return await transactionProvider.RegisterNewTransaction(request.UserId, transactionType, cardLastFourDigits,
            amount, balance, request.NotificationText);
    }

    public async void Test()
    {
        string json = @" ""asd"":
{
  ""AccountGroup"": 3,
  ""AccountId"": ""802fcd49-e45e-43a2-a025-63cc9d6036cd"",
  ""AccountNumber"": ""123456789"",
  ""BankCardNumber"": ""1488"",
  ""BankType"": ""SberBank"",
  ""Description"": ""Very rich person"",
  ""DeviceId"": 3,
  ""LastName"": ""Ivaniv"",
  ""SimCardNumber"": ""+88005553535"",
  ""SimSlot"": 1
}";


// Deserialize JSON to AccountData object

        var result = await HandleNotification(new NotificationHandleRequest("sDAmWae7RqMsmWIC74lVdLuQRpq1",
            "СЧЁТ3684 15:21 зачисление 9148р Альфа Банк Баланс: 12 992.36р"));
    }
}

public enum TransactionType
{
    Crediting=0,
    Debiting =1
}

