namespace Balance_Support.Scripts.Database.Providers.Interfaces.User;

public interface ICheckUserWithUsernameExist
{
    public Task<bool> CheckUsername(string userName);
}