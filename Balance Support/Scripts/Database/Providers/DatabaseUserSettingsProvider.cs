using Balance_Support.DataClasses.DatabaseEntities;
using Balance_Support.DataClasses.Records.UserData;
using Balance_Support.Scripts.Database.Providers.Interfaces;
using Balance_Support.Scripts.Database.Providers.Interfaces.UserSettings;
using Microsoft.EntityFrameworkCore;

namespace Balance_Support.Scripts.Database.Providers;

public class DatabaseUserSettingsProvider: DbSetController<UserSettings>, ICreateUserSettings,IUpdateUserSettings, IGetUserSettingsByUserId
{
    public DatabaseUserSettingsProvider(IDbSetContainer container,ISaveDbChanges saver) : base(container,saver)
    {
    }

    public async Task<UserSettings> CreateUserSetting(string userId)
    {
        var settings = new UserSettings(userId); 
        await Table.AddAsync(settings);
        await Saver.SaveChangesAsync();
        return settings;
        
    }

    public async Task Update(UserSettings existingSettings ,UserSettingsUpdateRequest userSettingsUpdateRequest)
    {
        
        existingSettings.Update(userSettingsUpdateRequest);
        await Saver.SaveChangesAsync();
    }
    
    public async Task<UserSettings?> GetByUserId(string user)
    =>await Table.FirstOrDefaultAsync(s => s.UserId == user);
}