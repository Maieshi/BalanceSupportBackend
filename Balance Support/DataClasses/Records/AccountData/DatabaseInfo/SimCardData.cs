namespace Balance_Support.DataClasses.Records.AccountData;

public record SimCardData(
    string SimCardId,
    string SimCardNumber,
    string BankType,
    int CardNumber,
    float InitalBalance
);