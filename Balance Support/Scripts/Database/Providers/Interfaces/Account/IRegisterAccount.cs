using Balance_Support.DataClasses.Records.AccountData;

namespace Balance_Support.Scripts.Database.Providers.Interfaces.Account;

public interface IRegisterAccount
{
    public Task<DataClasses.DatabaseEntities.Account?> RegisterAccount(AccountRegisterRequest request);
}