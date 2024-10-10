namespace Balance_Support.Scripts.Database.Providers.Interfaces.User;

public interface IGetUser
{
    public Task<DataClasses.DatabaseEntities.User?> GetUser(string userCred);
}