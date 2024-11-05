using Balance_Support.DataClasses.Records.UserData;
using Balance_Support.Scripts.Database.Providers;
using Balance_Support.Scripts.Database.Providers.Interfaces.User;
using Balance_Support.Scripts.Database.Providers.Interfaces.UserSettings;

namespace Balance_Support.Scripts.Controllers.Interfaces;

public interface IUserController
{
    public Task<IResult> RegisterNewUser(UserRegisterRequest userRegisterRequest);

    public Task<IResult> LogInUser(UserLoginRequest loginRequest, HttpContext context, LoginDeviceType deviceType);

    public Task<IResult> LogOutUser(HttpContext context);
}