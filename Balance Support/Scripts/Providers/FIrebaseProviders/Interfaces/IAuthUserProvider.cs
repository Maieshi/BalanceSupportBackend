namespace Balance_Support.Interfaces;

public interface IAuthUserProvider
{
    public Task<IResult> RegisterNewUser(string username, string email, string pasword);

    public Task<IResult> LogInUser(HttpContext context, string userCred, string password, LoginDeviceType deviceType);
    
    public Task<IResult> LogOutUser();
}