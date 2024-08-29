namespace Balance_Support.DataClasses.Records.AccountData;


public record AccountDataRequest(
    string AccountNumber,
    string LastName,
    int AccountGroup,
    int DeviceId,
    int SimSlot,
    string Description
);

public record AccountData(
    string AccountId,
    string AccountNumber,
    string LastName,
    int AccountGroup,
    int DeviceId,
    int SimSlot,
    string Description
);

