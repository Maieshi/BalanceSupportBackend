using Balance_Support.DataClasses.Records.AccountData;
using Balance_Support.DataClasses.DatabaseEntities;
namespace Balance_Support.Scripts.Database.Providers.Interfaces.Account;

public interface IUpdateAccount
{
    public Task UpdateAccount(DataClasses.DatabaseEntities.Account account, AccountUpdateRequest accountUpdateRequest);
}