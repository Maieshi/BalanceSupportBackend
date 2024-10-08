namespace Balance_Support.Scripts.Providers.Interfaces;

public interface IAuthUserProvider
{
    public Task<IResult> RegisterNewUser(string username, string email, string pasword);

    public Task<IResult> LogInUser(HttpContext context, string userCred, string password, LoginDeviceType deviceType);

    public Task<IResult> LogOutUser(HttpContext context);
}