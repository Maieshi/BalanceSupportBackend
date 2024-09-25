namespace Balance_Support.DataClasses.Records.UserData;

public record UserSettingsUpdateRequest(string UserId, int SelectedGroup, int RowsCount);