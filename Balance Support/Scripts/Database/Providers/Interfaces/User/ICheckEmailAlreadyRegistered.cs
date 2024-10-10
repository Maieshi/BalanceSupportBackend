namespace Balance_Support.Scripts.Database.Providers.Interfaces.User;

public interface ICheckEmailAlreadyRegistered
{
    public Task<bool> CheckEmail(string email);
}