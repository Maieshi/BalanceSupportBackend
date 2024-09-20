using Balance_Support.DataClasses.DatabaseEntities;
using Balance_Support.DataClasses.Records.AccountData;

namespace Balance_Support.Scripts.Providers.Interfaces;

public interface IDatabaseAccountProvider
{
    public Task<IResult> RegisterAccount(AccountRegisterRequest accountRegisterRequest);
    public Task<IResult> UpdateAccount(AccountUpdateRequest accountUpdateRequest);
    public Task<IResult> DeleteDevice(AccountDeleteRequest accountDeleteRequest);
    public Task<IResult> GetAccountsForDevice(AccountGetForDeviceRequest accountGetRequest);
    public Task<IResult> GetAllAccountsForUser(AccountGetAllForUserRequest accountGetAllForUserRequest);
    public Task<Account?> GetAccountByUserIdAndBankCardNumber(string userId, string bankCardNumber);
    public void Test();
}