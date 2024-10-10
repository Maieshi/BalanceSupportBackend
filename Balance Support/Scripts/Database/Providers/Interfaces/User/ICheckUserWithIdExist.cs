namespace Balance_Support.Scripts.Database.Providers.Interfaces.User;

public interface ICheckUserWithIdExist
{
    public Task<bool> CheckId(string userId);
}