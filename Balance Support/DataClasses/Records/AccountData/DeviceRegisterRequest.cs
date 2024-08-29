namespace Balance_Support.DataClasses.Records.AccountData;

public record DeviceRegisterRequest(
    string UserId,
    AccountDataRequest AccountData
);