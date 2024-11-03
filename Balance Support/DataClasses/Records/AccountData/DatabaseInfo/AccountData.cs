namespace Balance_Support.DataClasses.Records.AccountData;


public record AccountDataRequest(
    string AccountNumber,
    string LastName,
    int AccountGroup,
    int DeviceId,
    int SimSlot,
    string SimCardNumber,
    string BankCardNumber,
    string BankType,
    decimal InitialBalance,
    string Description
);
