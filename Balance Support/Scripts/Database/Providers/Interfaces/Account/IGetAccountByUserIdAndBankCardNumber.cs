namespace Balance_Support.Scripts.Database.Providers.Interfaces.Account;

public interface IGetAccountByUserIdAndBankCardNumber
{
    public  Task<DataClasses.DatabaseEntities.Account?> GetAccountByUserIdAndBankCardNumber(string userId, string bankCardNumber);
}