using Balance_Support.DataClasses.DatabaseEntities;
using Balance_Support.DataClasses.Records.UserData;
using Balance_Support.Scripts.Controllers.Interfaces;
using Balance_Support.Scripts.Database.Providers.Interfaces.UserSettings;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Balance_Support.Scripts.Controllers;

public class UserSettingsController: IUserSettingsController
{
    public async Task<IResult> GetUserSettings(UserSettingsGetRequest userSettingsGetRequest,
        IGetUserSettingsByUserId getUserSettingsByUserId)
    {
        var userSettings = await getUserSettingsByUserId.GetByUserId(userSettingsGetRequest.UserId);

        if (userSettings == null)
        {
            return Results.NotFound("Settings not found");
        }

        return Results.Ok(userSettings);
    }

    public async Task<IResult> UpdateUserSettings(UserSettingsUpdateRequest userSettingsUpdateRequest,
        IGetUserSettingsByUserId getUserSettingsByUserId, IUpdateUserSettings updateUserSettings)
    {
        var existingSettings = await getUserSettingsByUserId.GetByUserId(userSettingsUpdateRequest.UserId);

        if (existingSettings == null)
        {
            return Results.NotFound("User");
        }

        try
        {
            await updateUserSettings.Update(existingSettings, userSettingsUpdateRequest);
        }
        catch
        {
            return Results.Problem(statusCode: 500, title: "Error updating");
        }

        return Results.Ok("User settings updated successfully");
    }
}