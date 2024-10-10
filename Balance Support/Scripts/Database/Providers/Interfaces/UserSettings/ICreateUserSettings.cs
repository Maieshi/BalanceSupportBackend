namespace Balance_Support.Scripts.Database.Providers.Interfaces.UserSettings;

public interface ICreateUserSettings
{
    public  Task<DataClasses.DatabaseEntities.UserSettings> CreateUserSetting(string userId);
}