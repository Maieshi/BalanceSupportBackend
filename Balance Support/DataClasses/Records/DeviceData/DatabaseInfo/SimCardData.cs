namespace Balance_Support.DataClasses.Records.DeviceData;

public record SimCardData(
    string SimCardId,
    string SimCardNumber,
    string BankType,
    int CardNumber,
    float InitalBalance
);