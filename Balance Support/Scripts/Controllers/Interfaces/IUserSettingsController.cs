using Balance_Support.DataClasses.Records.UserData;
using Balance_Support.Scripts.Database.Providers.Interfaces.UserSettings;

namespace Balance_Support.Scripts.Controllers.Interfaces;

public interface IUserSettingsController
{
    public Task<IResult> GetUserSettings(UserSettingsGetRequest userSettingsGetRequest,
        IGetUserSettingsByUserId getUserSettingsByUserId);

    public Task<IResult> UpdateUserSettings(UserSettingsUpdateRequest userSettingsUpdateRequest,
        IGetUserSettingsByUserId getUserSettingsByUserId, IUpdateUserSettings updateUserSettings);
}