namespace Balance_Support.DataClasses.Records.DeviceData;

public record DeviceRegisterRequest(
    string UserId,
    DeviceData DeviceData,
    List<SimCardData> SimcardsData
);