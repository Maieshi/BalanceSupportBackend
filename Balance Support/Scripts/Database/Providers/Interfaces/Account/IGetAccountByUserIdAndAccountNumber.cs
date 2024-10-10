namespace Balance_Support.Scripts.Database.Providers.Interfaces.Account;

public interface IGetAccountByUserIdAndAccountNumber
{
    public  Task<DataClasses.DatabaseEntities.Account?> GetAccountByUserIdAndAccountNumber(string userId, string accountNumber);
}