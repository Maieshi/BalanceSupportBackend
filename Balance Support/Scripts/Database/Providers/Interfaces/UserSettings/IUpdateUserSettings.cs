using Balance_Support.DataClasses.Records.UserData;

namespace Balance_Support.Scripts.Database.Providers.Interfaces.UserSettings;

public interface IUpdateUserSettings
{
    public Task Update(DataClasses.DatabaseEntities.UserSettings existingSettings, UserSettingsUpdateRequest userSettingsUpdateRequest);
}