namespace Balance_Support.Scripts.Database.Providers.Interfaces.Account;

public interface IFindAccountByAccountId
{
    public  Task<DataClasses.DatabaseEntities.Account?> FindAccountByAccountId(string accountId);
}