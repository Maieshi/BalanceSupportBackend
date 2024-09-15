using Balance_Support.SerializationClasses;

namespace Balance_Support.Interfaces;

public interface IDatabaseUserProvider
{
    public Task<string> CreateNewUserAsync(UserAuthData newUser);

    public Task<UserAuthData?> GetUser(string userCred);
    public Task<bool> IsEmailAlreadyRegistered(string email);

    public Task<bool> IsUserWithIdExist(string userId);
    // public bool TryGetUser(string userCred, out UserAuthData user);
    //
    // public bool TryGetUser(string recordId, out UserAuthData user);

    //TODO:add IsUserExist
}