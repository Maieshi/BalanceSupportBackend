using Balance_Support.DataClasses.DatabaseEntities;

namespace Balance_Support.DataClasses.Records.NotificationData;
public record MessagesGetRequest(
    string UserId,
    string? SearchText,
    string? AccountNumber,
    DateTime? StartingDate,
    DateTime? EndingDate,
    int? MessageType,
    int Amount
)
{
    public bool Matches(Transaction transaction, string accountNumber)
    {
        // Check if transaction UserId matches
        if (transaction.UserId != UserId) return false;

        // Check if AccountNumber is specified and matches
        if (!string.IsNullOrEmpty(AccountNumber) && accountNumber != AccountNumber) return false;

        // Check if SearchText is specified and contained within the message
        if (!string.IsNullOrEmpty(SearchText) && !transaction.Message.Contains(SearchText, StringComparison.OrdinalIgnoreCase))
            return false;

        // Check if StartingDate is specified and transaction time is after or on the StartingDate
        if (StartingDate.HasValue && transaction.Time < StartingDate.Value) return false;

        // Check if EndingDate is specified and transaction time is before or on the EndingDate
        if (EndingDate.HasValue && transaction.Time > EndingDate.Value) return false;

        // Check if MessageType is specified and matches transaction type
        if (MessageType.HasValue && MessageType.Value != -1 && transaction.TransactionType != MessageType.Value) return false;

        // If all checks pass, the transaction matches
        return true;
    }
}
