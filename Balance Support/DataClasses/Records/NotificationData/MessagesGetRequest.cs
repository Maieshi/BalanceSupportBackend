namespace Balance_Support.DataClasses.Records.NotificationData;

public record MessagesGetRequest(
    string UserId,
    string? SearchText,
    string? AccountNumber,
    DateTime? StartingDate,
    DateTime? EndingDate,
    int? MessageType,
    int Amount
    );