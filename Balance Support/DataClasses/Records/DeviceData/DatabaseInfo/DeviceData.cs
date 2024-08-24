namespace Balance_Support.DataClasses.Records.DeviceData;

public record DeviceData(
    
    string DeviceId,
    string LastName,
    int DeviceGroup,
    int DeviceSubgroup,
    string Description
);