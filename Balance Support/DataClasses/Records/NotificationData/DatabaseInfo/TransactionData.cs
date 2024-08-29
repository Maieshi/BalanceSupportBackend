namespace Balance_Support.DataClasses.Records.NotificationData.DatabaseInfo;

public record TransactionData(
     string AccountId ,
     TransactionType TransactionType ,
     decimal Amount,
     decimal Balance,
     string Message
);