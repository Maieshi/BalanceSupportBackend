namespace Balance_Support.DataClasses.Records.DeviceData;

public record DeviceDeleteRequest(
    string UserId,
    string DeviceId
);