namespace Balance_Support.DataClasses.Records.AccountData;

public record AccountRegisterRequest(
    string UserId,
    AccountDataRequest AccountData
);