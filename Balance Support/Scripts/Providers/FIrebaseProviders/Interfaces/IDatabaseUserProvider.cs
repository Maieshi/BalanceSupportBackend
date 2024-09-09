using Balance_Support.DataClasses;
using Balance_Support.SerializationClasses;
// using Firebase.Auth;

namespace Balance_Support.Interfaces;

public interface IDatabaseUserProvider
{
    public Task<(bool IsSuccess, string? ErrorMessage)> CreateUserAsync(User newUser);
    
    public Task<User?> GetUser(string userCred);
    public Task<bool> IsEmailAlreadyRegistered(string email);
    
    public Task<bool> IsUserWithIdExist(string userId);
    // public bool TryGetUser(string userCred, out UserAuthData user);
    //
    // public bool TryGetUser(string recordId, out UserAuthData user);

    //TODO:add IsUserExist
}