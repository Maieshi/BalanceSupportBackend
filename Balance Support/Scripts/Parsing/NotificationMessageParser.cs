using System.Text.RegularExpressions;
using Balance_Support.DataClasses.Records.NotificationData;
using Balance_Support.Scripts.Database.Providers.Interfaces;

namespace Balance_Support.Scripts.Parsing;

public class NotificationMessageParser : INotificationMessageParser
{
    public async Task<TransactionParsedData?> HandleNotification(NotificationHandleRequest request)
    {
        // Match card number patterns like СЧЁТ0958, MIR-5071, or Карта *XXXX
        var cardNumberMatch = Regex.Match(request.NotificationText, @"\b(?:MIR-|СЧЁТ|Карта \*|)(\d{4})\b");

        if (!cardNumberMatch.Success)
        {
            return null;
        }

        string cardLastFourDigits = cardNumberMatch.Groups[1].Value;

        // Match transaction types and amounts, e.g., зачисление 320р or списание 5000р
        var transactionMatch = Regex.Match(request.NotificationText,
            @"(?:(зачисление|Перевод из|перевод|списание)\s+([\d.,]+)(?:р|\+))");

        if (!transactionMatch.Success || !decimal.TryParse(
                transactionMatch.Groups[2].Value.Replace(",", "."),
                System.Globalization.NumberStyles.Number,
                System.Globalization.CultureInfo.InvariantCulture,
                out var amount))
        {
            return null;
        }

        // Determine transaction type (crediting or debiting)
        TransactionType transactionType = transactionMatch.Groups[1].Value switch
        {
            "зачисление" or "Перевод из" => TransactionType.Crediting,
            "перевод" or "списание" => TransactionType.Debiting,
            _ => throw new ArgumentOutOfRangeException()
        };

        // Match the balance pattern, either 'Баланс: XXXXXр' or 'Доступно XXXXXр'
        var balanceMatch = Regex.Match(request.NotificationText, @"(?:Баланс|Доступно):?\s*([\d\s]+(?:,\d{1,2})?\.\d{1,2})р");
        decimal balance = 0;

        if (!balanceMatch.Success || !decimal.TryParse(
                balanceMatch.Groups[1].Value.Replace(" ", ""),
                System.Globalization.NumberStyles.Number,
                System.Globalization.CultureInfo.InvariantCulture,
                out balance))
        {
            return null;
        }

        // Return parsed data
        return new TransactionParsedData(transactionType, cardLastFourDigits, amount, balance);
    }
}

public enum TransactionType
{
    Crediting = 0,
    Debiting = 1
}

public record TransactionParsedData(TransactionType Type, string CardNumber, decimal Amount, decimal Balance);
