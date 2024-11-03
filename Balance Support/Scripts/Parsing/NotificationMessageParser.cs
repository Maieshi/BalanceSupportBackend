using System.Globalization;
using System.Text.RegularExpressions;
using Balance_Support.DataClasses.Records.NotificationData;
using Balance_Support.Scripts.Database.Providers.Interfaces;
using System.Globalization;
using System.Text.RegularExpressions;
namespace Balance_Support.Scripts.Parsing;

public class NotificationMessageParser : INotificationMessageParser
{

   public async Task<TransactionParsedData?> HandleNotification(NotificationHandleRequest request)
{
    var message = request.NotificationText;
    string bank = DetectBank(message);
    if (string.IsNullOrEmpty(bank))
    {
        return null; // Unknown bank, skip parsing
    }

    // Define regex patterns for each bank
    Regex creditingPattern = null;
    Regex debitingPattern = null;

    if (bank == "Sberbank")
    {
        creditingPattern = new Regex(@"^(СЧЁТ|MIR)?[\s*-]?(\d{4,})?\s+\d{2}:\d{2}\s+(?:зачисление|Перевод из).+?([\d\s,.]+)[рRUR].*?Баланс:\s*([\d\s,.]+)[рRUR]", RegexOptions.IgnoreCase);
        debitingPattern = new Regex(@"^(СЧЁТ|MIR)?[\s*-]?(\d{4,})?\s+\d{2}:\d{2}\s+(?:перевод).+?([\d\s,.]+)[рRUR].*?Баланс:\s*([\d\s,.]+)[рRUR]", RegexOptions.IgnoreCase);
    }
    else if (bank == "AlphaBank")
    {
        creditingPattern = new Regex(@"Пополнение\s*\*?(\d{4})?\s*на\s*([\d\s,.]+)\s*[рRUR].*?Баланс:\s*([\d\s,.]+)", RegexOptions.IgnoreCase);
    }
    else if (bank == "OTP Bank")
    {
        creditingPattern =  new Regex(@"Карта\s*\*?(\d{4})\s+зачисление\s+([\d\s,.]+)[рpRUR].*?Доступно\s+([\d\s,.]+)[рpRUR]", RegexOptions.IgnoreCase);
        debitingPattern = new Regex(@"Карта\s*\*?(\d{4})\s+списание\s+([\d\s,.]+)[рpRUR].*?Доступно\s+([\d\s,.]+)[рpRUR]", RegexOptions.IgnoreCase);
    }
    else
    {
        return null; // If the bank is not recognized, skip parsing
    }

    // Match the message against crediting and debiting patterns
    Match match;
    TransactionType type;

    // Check for crediting first
    if ((match = creditingPattern.Match(message)).Success)
    {
        type = TransactionType.Crediting;
    }
    // Check for debiting if crediting is not found
    else if (debitingPattern!=null&& (match = debitingPattern.Match(message)).Success)
    {
        type = TransactionType.Debiting;
    }
    else
    {
        return null; // Unknown transaction type
    }

    return ParseTransactionData(match, bank, type);
}


private static string DetectBank(string message)
{
    if (message.Contains("СЧЁТ") || message.Contains("MIR") || message.Contains("Сбербанк", StringComparison.OrdinalIgnoreCase))
    {
        return "Sberbank";
    }
    if (message.Contains("Альфа Банк", StringComparison.OrdinalIgnoreCase) || message.Contains("по СБП") || message.Contains("Пополнение", StringComparison.OrdinalIgnoreCase))
    {
        return "AlphaBank";
    }
    if (message.Contains("OTP Bank", StringComparison.OrdinalIgnoreCase) || message.Contains("otpbank.ru"))
    {
        return "OTP Bank";
    }
    return string.Empty;
}

private static TransactionParsedData ParseTransactionData(Match match, string bank, TransactionType type)
{
    int cardIndex = bank switch 
    {
         "Sberbank"=>  2,
         "AlphaBank"=> 1,
         "OTP Bank"=>  1,
         _=>  2
    };
    
    int amountIndex = bank switch 
    {
        "Sberbank"=>  3,
        "AlphaBank"=> 2,
        "OTP Bank"=>  2,
        _=>  3
    };
    
    int balanceIndex = bank switch 
    {
        "Sberbank"=>  4,
        "AlphaBank"=> 3,
        "OTP Bank"=>  3,
        _=>  4
    };
    
    // Capture card/account number, or leave as empty string if not present
    var cardNumber = match.Groups[cardIndex].Success ? match.Groups[cardIndex].Value.Trim() : string.Empty;

    // Parse the amount and balance while handling different formats
    decimal.TryParse(RemoveUnnecessarySymbols(match.Groups[amountIndex].Value), NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var amount);
    decimal.TryParse(RemoveUnnecessarySymbols(match.Groups[balanceIndex].Value), NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var balance);

    return new TransactionParsedData(bank, type, cardNumber, amount, balance);
}

private static string RemoveUnnecessarySymbols(string input)
{
    return input.Replace(" ", "").Replace("\u00A0", "").Replace(",","."); // Replace both regular spaces and non-breaking spaces
}
}


public enum TransactionType
{
    Crediting = 0,
    Debiting = 1
}

public record TransactionParsedData(string Bank, TransactionType Type, string CardNumber, decimal Amount, decimal Balance);

