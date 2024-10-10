namespace Balance_Support.Scripts.Database.Providers.Interfaces.Account;

public interface IDeleteAccount
{
    public Task Delete(DataClasses.DatabaseEntities.Account account);
}