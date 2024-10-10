using Balance_Support.Scripts.Database.Providers;
using Balance_Support.Scripts.Parsing;

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