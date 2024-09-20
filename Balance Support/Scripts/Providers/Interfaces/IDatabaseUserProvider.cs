using Balance_Support.DataClasses.DatabaseEntities;

namespace Balance_Support.Scripts.Providers.Interfaces;

public interface IDatabaseUserProvider
{
    public Task<(bool IsSuccess, string? ErrorMessage)> CreateUserAsync(User newUser);
    
    public Task<User?> GetUser(string userCred);
    public Task<bool> IsEmailAlreadyRegistered(string email);
    
    public Task<bool> IsUserWithIdExist(string userId);

    public Task<bool> IsUserWithUsernameExist(string userName);
    
}