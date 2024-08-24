namespace Balance_Support.DataClasses.Records.DeviceData;

public record DeviceUpdateRequest
(
    string UserId,
    string DeviceId,
    DeviceData DeviceData
);