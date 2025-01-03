namespace Balance_Support.Scripts.Database.Providers.Interfaces.User;

public interface IDatabaseUserProvider
{
    public Task<bool> CheckUserWithEmailExist(string email);
    public Task<bool> CheckUserWithIdExist(string userId);
    public Task<bool> CheckUserWithUsernameExist(string userName);
    public Task<DataClasses.DatabaseEntities.User?> GetUser(string userCred);
    public Task RegisterUser(string userId, string email, string displayName);
}