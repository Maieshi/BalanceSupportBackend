namespace Balance_Support.DataClasses.Records.NotificationData.DatabaseInfo;

public record TransactionData(
     string UserId,
     string AccountId ,
     TransactionType TransactionType ,
     decimal Amount,
     decimal Balance,
     DateTime Time,
     string Message
);