namespace Balance_Support.DataClasses.Records.DeviceData;

public record UserDeviceSimСardRelationData(
    string UserId,
    string DeviceRecordId,
    string DeviceId,
    string SimCardRecordId,
    string SimCardId);