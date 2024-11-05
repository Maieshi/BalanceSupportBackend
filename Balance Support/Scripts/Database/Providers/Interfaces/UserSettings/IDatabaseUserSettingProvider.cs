using Balance_Support.DataClasses.Records.UserData;

namespace Balance_Support.Scripts.Database.Providers.Interfaces.UserSettings;

public interface IDatabaseUserSettingProvider
{
    public  Task<DataClasses.DatabaseEntities.UserSettings> CreateUserSetting(string userId);
    public Task<DataClasses.DatabaseEntities.UserSettings?> GetByUserId(string user);
    public Task Update(DataClasses.DatabaseEntities.UserSettings existingSettings, UserSettingsUpdateRequest userSettingsUpdateRequest);
}