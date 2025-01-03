using Balance_Support.DataClasses.Records.AccountData;

namespace Balance_Support.Scripts.Database.Providers.Interfaces.Account;

public interface IDatabaseAccountProvider
{
    public Task Delete(DataClasses.DatabaseEntities.Account account);
    public  Task<DataClasses.DatabaseEntities.Account?> FindAccountByAccountId(string accountId);
    public  Task<List<DataClasses.DatabaseEntities.Account>> FindAccountsByUserId(string userId, bool includeDeleted = false);
    public  Task<DataClasses.DatabaseEntities.Account?> GetAccountByUserIdAndBankCardNumber(string userId, string bankCardNumber);
    public Task<List<DataClasses.DatabaseEntities.Account>> GetAccountsForUserSelectedGroupAndIsDeleted(string userId, List<int>? selectedGroup=null, bool includeDeleted = false);
    public  Task<List<DataClasses.DatabaseEntities.Account>> GetAccountByUserGroupDevice(string userId, int groupId, int deviceId);

    public Task<DataClasses.DatabaseEntities.Account?> GetAccountByUserIdNameSimBankType(string userId, string lastName, string SimCard, string bankType);
    public Task<DataClasses.DatabaseEntities.Account?> RegisterAccount(AccountRegisterRequest request);
    
    public Task<DataClasses.DatabaseEntities.Account?> FindAccountBySimCardAndBank(string userId, string simCard, string bank);
    public Task UpdateAccount(DataClasses.DatabaseEntities.Account account, AccountUpdateRequest accountUpdateRequest);
    public Task UpdateAccount(DataClasses.DatabaseEntities.Account account);
}