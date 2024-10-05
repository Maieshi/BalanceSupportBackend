using Balance_Support.DataClasses.Records.UserData;
using Balance_Support.Scripts.Providers.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Balance_Support.Scripts.Providers;

public class DatabaseUserSettingsProvider: IDatabaseUserSettingsProvider
{
    private readonly ApplicationDbContext context;

    public DatabaseUserSettingsProvider(ApplicationDbContext context)
    {
        this.context = context;
    }

    public async Task<bool> CreateUserSetting(string userId)
    {
        try
        {
            await context.UserSettings.AddAsync(new UserSettings(userId));
            await context.SaveChangesAsync();
        }
        catch (Exception e)
        {
            return false;
        }

        return true;
    }

    public async Task<IResult> UpdateUserSettings(UserSettingsUpdateRequest userSettingsUpdateRequest)
    {
        var existingSettings = await context.UserSettings.FirstOrDefaultAsync(s => s.UserId == userSettingsUpdateRequest.UserId);

        if (existingSettings == null)
        {
            return Results.NotFound("User");
        }

        existingSettings.Update(userSettingsUpdateRequest);

        try
        {
            await context.SaveChangesAsync();
        }
        catch
        {
            return Results.Problem(statusCode: 500, title: "Error saving");
        }

        return Results.Ok("User settings updated successfully");
    }

    public async Task<IResult> GetUserSettings(UserSettingsGetRequest userSettingsGetRequest)
    {
        var userSettings = await context.UserSettings.FirstOrDefaultAsync(s => s.UserId == userSettingsGetRequest.UserId);

        if (userSettings == null)
        {
            return Results.NotFound("Settings not found");
        }

        return Results.Ok(userSettings);
    }

    public async Task<UserSettings?> GetUserSettings(string user)
    =>await context.UserSettings.FirstOrDefaultAsync(s => s.UserId == user);
}