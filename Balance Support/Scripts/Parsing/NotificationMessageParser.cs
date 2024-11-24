using System.Globalization;
using System.Text.RegularExpressions;
using Balance_Support.DataClasses.Records.NotificationData;
using Balance_Support.Scripts.Database.Providers.Interfaces;
using System.Globalization;
using System.Text.RegularExpressions;
using Balance_Support.DataClasses.DatabaseEntities;

namespace Balance_Support.Scripts.Parsing;

public class NotificationMessageParser : INotificationMessageParser
{
    public async Task<TransactionParsedData?> ParseMessage(NotificationHandleRequest request)
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
            creditingPattern =
                new Regex(
                    @"^(СЧЁТ|MIR)?[\s*-]?(\d{4,})?\s+\d{2}:\d{2}\s+(?:зачисление|Перевод из).+?([\d\s,.]+)[рRUR].*?Баланс:\s*([\d\s,.]+)[рRUR]",
                    RegexOptions.IgnoreCase);
            debitingPattern =
                new Regex(
                    @"^(СЧЁТ|MIR)?[\s*-]?(\d{4,})?\s+\d{2}:\d{2}\s+(?:перевод).+?([\d\s,.]+)[рRUR].*?Баланс:\s*([\d\s,.]+)[рRUR]",
                    RegexOptions.IgnoreCase);
        }
        else if (bank == "AlphaBank")
        {
            creditingPattern =
                new Regex(@"Пополнение\s*\*?(\d{4})?\s*на\s*([\d\s,.]+)\s*[рRUR].*?Баланс:\s*([\d\s,.]+)",
                    RegexOptions.IgnoreCase);
        }
        else if (bank == "OTP Bank")
        {
            creditingPattern =
                new Regex(@"Карта\s*\*?(\d{4})\s+зачисление\s+([\d\s,.]+)[рpRUR].*?Доступно\s+([\d\s,.]+)[рpRUR]",
                    RegexOptions.IgnoreCase);
            debitingPattern =
                new Regex(@"Карта\s*\*?(\d{4})\s+списание\s+([\d\s,.]+)[рpRUR].*?Доступно\s+([\d\s,.]+)[рpRUR]",
                    RegexOptions.IgnoreCase);
        }
        else if (bank == "MTS Money")
        {
            creditingPattern =
                new Regex(@"Перевод с карты\s+([\d\s,.]+)\s+RUB.*?Остаток:\s+([\d\s,.]+)\s+RUB;?\s*\*?(\d{4})?",
                    RegexOptions.IgnoreCase);


            debitingPattern = new Regex(@"([\d\s,.]+)\s+RUB.*?Остаток:\s+([\d\s,.]+)\s+RUB;?\s*\*?(\d{4})?",
                RegexOptions.IgnoreCase);
            var alternateDebitingPattern = new Regex(@"Выполнен перевод\s+([\d\s,.]+)[рRUR].*?со счёта\s*\*?(\d{4})",
                RegexOptions.IgnoreCase);

            Match messageMatch = alternateDebitingPattern.Match(message);
            if (messageMatch.Success) debitingPattern = alternateDebitingPattern;
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
        else if (debitingPattern != null && (match = debitingPattern.Match(message)).Success)
        {
            type = TransactionType.Debiting;
        }
        else
        {
            return null; // Unknown transaction type
        }

        return ParseTransactionData(match, bank, type);
    }

    public async Task<TransactionParsedData?> ParseSimpleMessage(Account account, string message)
    {
        if (account == null || string.IsNullOrWhiteSpace(message))
            return null;

        string bank = account.BankType;
        string cardNumber = account.BankCardNumber ?? string.Empty;
        decimal amount = 0;
        var type = TransactionType.Crediting; // Default to crediting

        if (bank == "Alfa")
        {
            // Parse AlphaBank crediting messages
            var alphaPattern = new Regex(@"Поступление\s+([\d,.]+)\s+[рRUR]", RegexOptions.IgnoreCase);

            var match = alphaPattern.Match(message);

            if (match.Success)
            {
                decimal.TryParse(RemoveUnnecessarySymbols(match.Groups[1].Value), NumberStyles.AllowDecimalPoint,
                    CultureInfo.InvariantCulture, out amount);
            }
        }
        else if (bank == "MTC")
        {
            // Parse MTS Money crediting messages
            var mtsPattern = new Regex(@"Поступление\s+([\d\s,.]+)[рRUR].*?через\s+СБП", RegexOptions.IgnoreCase);
            var match = mtsPattern.Match(message);

            if (match.Success)
            {
                decimal.TryParse(RemoveUnnecessarySymbols(match.Groups[1].Value), NumberStyles.AllowDecimalPoint,
                    CultureInfo.InvariantCulture, out amount);
            }
        }

        // If no amount was parsed, return null
        if (amount == 0)
            return null;

        // Create and return the transaction parsed data
        return new TransactionParsedData(bank, type, cardNumber, amount, 0); // Balance is 0 as per requirements
    }

    private static string DetectBank(string message)
    {
        if (message.Contains("СЧЁТ") || message.Contains("MIR") ||
            message.Contains("Сбербанк", StringComparison.OrdinalIgnoreCase))
        {
            return "Sberbank";
        }

        if (message.Contains("Альфа Банк", StringComparison.OrdinalIgnoreCase) || message.Contains("по СБП") ||
            message.Contains("Пополнение", StringComparison.OrdinalIgnoreCase))
        {
            return "AlphaBank";
        }

        if (message.Contains("OTP Bank", StringComparison.OrdinalIgnoreCase) || message.Contains("otpbank.ru"))
        {
            return "OTP Bank";
        }

        if ((message.Contains("RUB", StringComparison.OrdinalIgnoreCase) &&
             message.Contains("Остаток", StringComparison.OrdinalIgnoreCase)) ||
            message.Contains("Выполнен перевод"))
        {
            return "MTS Money";
        }

        return string.Empty;
    }

    private static TransactionParsedData ParseTransactionData(Match match, string bank, TransactionType type)
    {
        int cardIndex = bank switch
        {
            "Sberbank" => 2,
            "AlphaBank" => 1,
            "OTP Bank" => 1,
            "MTS Money" => 3,
            _ => 2
        };

        int amountIndex = bank switch
        {
            "Sberbank" => 3,
            "AlphaBank" => 2,
            "OTP Bank" => 2,
            "MTS Money" => 1,
            _ => 3
        };

        int balanceIndex = bank switch
        {
            "Sberbank" => 4,
            "AlphaBank" => 3,
            "OTP Bank" => 3,
            "MTS Money" => 2,
            _ => 4
        };

        if (bank == "MTS Money" && type == TransactionType.Debiting && match.Groups.Count == 3)
        {
            cardIndex = 2;
            balanceIndex = 0;
        }

        // Capture card/account number, or leave as empty string if not present
        var cardNumber = match.Groups[cardIndex].Success ? match.Groups[cardIndex].Value.Trim() : string.Empty;

        // Parse the amount and balance while handling different formats
        decimal.TryParse(RemoveUnnecessarySymbols(match.Groups[amountIndex].Value), NumberStyles.AllowDecimalPoint,
            CultureInfo.InvariantCulture, out var amount);
        decimal.TryParse(RemoveUnnecessarySymbols(match.Groups[balanceIndex].Value), NumberStyles.AllowDecimalPoint,
            CultureInfo.InvariantCulture, out var balance);

        return new TransactionParsedData(bank, type, cardNumber, amount, balance);
    }

    private static string RemoveUnnecessarySymbols(string input)
    {
        return input.Replace(" ", "").Replace("\u00A0", "").Replace(",", ".")
            .Trim(); // Replace both regular spaces and non-breaking spaces
    }
}

public enum TransactionType
{
    Crediting = 0,
    Debiting = 1
}

public record TransactionParsedData(
    string Bank,
    TransactionType Type,
    string CardNumber,
    decimal Amount,
    decimal Balance);