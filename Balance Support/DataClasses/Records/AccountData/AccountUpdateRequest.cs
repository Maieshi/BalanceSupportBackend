namespace Balance_Support.DataClasses.Records.AccountData;

public record AccountUpdateRequest
(
    string UserId,
    string AccountId,
    AccountDataRequest AccountData
);