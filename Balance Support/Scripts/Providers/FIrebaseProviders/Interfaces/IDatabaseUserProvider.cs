using Balance_Support.DataClasses;
using Balance_Support.DataClasses.DatabaseEntities;
using Balance_Support.SerializationClasses;

namespace Balance_Support.Interfaces;

public interface IDatabaseUserProvider
{
    public Task<(bool IsSuccess, string? ErrorMessage)> CreateUserAsync(User newUser);
    
    public Task<User?> GetUser(string userCred);
    public Task<bool> IsEmailAlreadyRegistered(string email);
    
    public Task<bool> IsUserWithIdExist(string userId);

    public Task<bool> IsUserWithUsernameExist(string userName);
    
}