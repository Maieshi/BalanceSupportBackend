namespace Balance_Support.DataClasses.Records.DeviceData;

public record DeviceUpdateRequest
(
    string DeviceId,
    DeviceData DeviceData
);