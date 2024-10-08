using Balance_Support.DataClasses.Records.AccountData;

namespace Balance_Support.Scripts.Providers.Interfaces.Account;

public interface IRegisterAccount
{
    public Task RegisterAccount(AccountRegisterRequest request);
}