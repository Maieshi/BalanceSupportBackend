namespace Balance_Support.Scripts.Database.Providers.Interfaces.Account;

public interface IFindAccountsByUserId
{
    public  Task<List<DataClasses.DatabaseEntities.Account>> FindAccountsByUserId(string userId);
}