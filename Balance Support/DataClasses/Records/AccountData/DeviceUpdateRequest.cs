namespace Balance_Support.DataClasses.Records.AccountData;

public record DeviceUpdateRequest
(
    string UserId,
    string AccountId,
    AccountDataRequest AccountDataRequest
);