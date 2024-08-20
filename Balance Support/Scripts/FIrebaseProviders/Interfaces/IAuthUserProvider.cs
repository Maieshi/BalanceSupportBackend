namespace Balance_Support.Interfaces;

public interface IAuthUserProvider
{
    public Task<IResult> RegisterNewUser(string username, string email, string pasword);

    public Task<IResult> LogInUser(string userRecordId, string userCred, string password, LoginDeviceType deviceType, HttpContext context);
    
    public Task<IResult> LogOutUser();
}