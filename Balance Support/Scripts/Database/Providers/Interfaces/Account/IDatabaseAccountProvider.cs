using Balance_Support.DataClasses.Records.AccountData;

namespace Balance_Support.Scripts.Database.Providers.Interfaces.Account;

public interface IDatabaseAccountProvider
{
    public Task<bool> CanProceedRequest(AccountDataRequest accountData, string userId, string? accountId = null);
    public Task Delete(DataClasses.DatabaseEntities.Account account);
    public  Task<DataClasses.DatabaseEntities.Account?> FindAccountByAccountId(string accountId);
    public  Task<List<DataClasses.DatabaseEntities.Account>> FindAccountsByUserId(string userId);
    public  Task<DataClasses.DatabaseEntities.Account?> GetAccountByUserIdAndAccountNumber(string userId, string accountNumber);
    public  Task<DataClasses.DatabaseEntities.Account?> GetAccountByUserIdAndBankCardNumber(string userId, string bankCardNumber);
    public  Task<List<DataClasses.DatabaseEntities.Account>> GetAccoutForDeivce(string userId, int accountGroup, int deviceId);
    public Task<List<DataClasses.DatabaseEntities.Account>> GetAllAccountsForUser(string userId, int? selectedGroup=null);
    public Task<DataClasses.DatabaseEntities.Account?> RegisterAccount(AccountRegisterRequest request);
    public Task UpdateAccount(DataClasses.DatabaseEntities.Account account, AccountUpdateRequest accountUpdateRequest);
    public Task UpdateAccount(DataClasses.DatabaseEntities.Account account);
}