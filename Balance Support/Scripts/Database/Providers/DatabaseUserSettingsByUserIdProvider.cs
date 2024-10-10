using Balance_Support.DataClasses.DatabaseEntities;
using Balance_Support.DataClasses.Records.UserData;
using Balance_Support.Scripts.Database.Providers.Interfaces;
using Balance_Support.Scripts.Database.Providers.Interfaces.UserSettings;
using Microsoft.EntityFrameworkCore;

namespace Balance_Support.Scripts.Database.Providers;

public class DatabaseUserSettingsByUserIdProvider: DbSetController<UserSettings>, ICreateUserSettings,IUpdateUserSettings, IGetUserSettingsByUserId
{
    public DatabaseUserSettingsByUserIdProvider(IDbSetContainer container,ISaveDbChanges saver) : base(container,saver)
    {
    }

    public async Task<UserSettings> CreateUserSetting(string userId)
    {
        var settings = new UserSettings(userId); 
        await Table.AddAsync(settings);
        await Saver.SaveChangesAsync();
        return settings;
        // try
        // {
        //     
        // }
        // catch (Exception e)
        // {
        //     return false;
        // }
        //
        // return true;
    }

    public async Task Update(UserSettings existingSettings ,UserSettingsUpdateRequest userSettingsUpdateRequest)
    {
        // var existingSettings = await Table.FirstOrDefaultAsync(s => s.UserId == userSettingsUpdateRequest.UserId);

        // if (existingSettings == null)
        // {
        //     return Results.NotFound("User");
        // }

        existingSettings.Update(userSettingsUpdateRequest);
        await Saver.SaveChangesAsync();
        // try
        // {
        //     
        // }
        // catch
        // {
        //     return Results.Problem(statusCode: 500, title: "Error saving");
        // }
        //
        // return Results.Ok("User settings updated successfully");
    }

    // public async Task<UserSettings?> GetUserSettings(UserSettingsGetRequest userSettingsGetRequest)
// =>            await Table.FirstOrDefaultAsync(s => s.UserId == userSettingsGetRequest.UserId);
    // {
        // var userSettings = 

        // if (userSettings == null)
        // {
        //     return Results.NotFound("Settings not found");
        // }
        //
        // return Results.Ok(userSettings);
    // }

    public async Task<UserSettings?> GetByUserId(string user)
    =>await Table.FirstOrDefaultAsync(s => s.UserId == user);
}