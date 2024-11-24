namespace Balance_Support.DataClasses.Records.NotificationData;

public record NotificationHandleRequest(string UserId, int GroupId, int DeviceId, string NotificationText);