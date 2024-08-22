using Balance_Support.SerializationClasses;

namespace Balance_Support.Interfaces;

public interface IDatabaseUserProvider
{
    public Task<string> CreateNewUserAsync(UserAuthData newUser);

    public bool TryGetUser(string userCred, out UserAuthData user);

    public bool TryGetUserByRecordId(string recordId, out UserAuthData user);
    
    //TODO:add IsUserExist
}