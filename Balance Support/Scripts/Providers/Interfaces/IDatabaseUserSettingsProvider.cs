using Balance_Support.DataClasses.Records.UserData;

namespace Balance_Support.Scripts.Providers.Interfaces;

public interface IDatabaseUserSettingsProvider
{
    public Task<bool> CreateUserSetting(string userId);
    public Task<IResult> UpdateUserSettings(UserSettingsUpdateRequest userSettingsUpdateRequest);
    public Task<IResult> GetUserSettings(UserSettingsGetRequest getRequest);
}