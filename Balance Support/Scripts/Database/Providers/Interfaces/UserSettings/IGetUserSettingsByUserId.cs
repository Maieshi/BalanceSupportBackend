namespace Balance_Support.Scripts.Database.Providers.Interfaces.UserSettings;

public interface IGetUserSettingsByUserId
{
    public Task<DataClasses.DatabaseEntities.UserSettings?> GetByUserId(string user);
}