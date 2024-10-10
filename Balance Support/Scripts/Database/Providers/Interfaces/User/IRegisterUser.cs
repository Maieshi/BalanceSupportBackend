namespace Balance_Support.Scripts.Database.Providers.Interfaces.User;

public interface IRegisterUser
{
    public Task RegisterUser(string userId, string email, string displayName);
}