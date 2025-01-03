using Balance_Support.DataClasses.DatabaseEntities;
using Balance_Support.DataClasses.Records.UserData;
using Balance_Support.Scripts.Controllers.Interfaces;
using Balance_Support.Scripts.Database.Providers.Interfaces.UserSettings;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Balance_Support.Scripts.Controllers;

public class UserSettingsController : IUserSettingsController
{
    private readonly IDatabaseUserSettingProvider userSettings;

    public UserSettingsController(IDatabaseUserSettingProvider userSettings)
    {
        this.userSettings = userSettings;
    }
    
    public async Task<IResult> GetUserSettings(UserSettingsGetRequest userSettingsGetRequest)
    {
        var userSettings = await this.userSettings.GetByUserId(userSettingsGetRequest.UserId);
        
        if (userSettings == null)
        {
            return Results.NotFound("Settings not found");
        }

        return Results.Ok(userSettings.Convert());
    }

    public async Task<IResult> UpdateUserSettings(UserSettingsUpdateRequest userSettingsUpdateRequest)
    {
        var existingSettings = await userSettings.GetByUserId(userSettingsUpdateRequest.UserId);

        if (existingSettings == null)
        {
            return Results.NotFound("User");
        }

        try
        {
            await userSettings.Update(existingSettings, userSettingsUpdateRequest);
        }
        catch
        {
            return Results.Problem(statusCode: 500, title: "Error updating");
        }

        return Results.Ok("User settings updated successfully");
    }
}