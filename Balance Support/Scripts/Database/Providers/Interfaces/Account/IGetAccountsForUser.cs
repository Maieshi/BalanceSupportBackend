namespace Balance_Support.Scripts.Database.Providers.Interfaces.Account;

public interface IGetAccountsForUser
{
    public Task<List<DataClasses.DatabaseEntities.Account>> GetAllAccountsForUser(string userId, int? selectedGroup=null);
}