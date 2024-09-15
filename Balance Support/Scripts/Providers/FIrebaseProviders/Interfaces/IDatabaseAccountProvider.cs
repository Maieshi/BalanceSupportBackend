using System.Collections.ObjectModel;
using Balance_Support.DataClasses.Records.AccountData;
using Firebase.Database;

namespace Balance_Support.Interfaces;

public interface IDatabaseAccountProvider
{
    public Task<IResult> RegisterAccount(AccountRegisterRequest accountRegisterRequest);
    public Task<IResult> UpdateAccount(AccountUpdateRequest accountUpdateRequest);
    public Task<IResult> DeleteDevice(AccountDeleteRequest accountDeleteRequest);
    public Task<IResult> GetAccountsForDevice(AccountGetForDeviceRequest accountGetForDeviceRequest);
    public Task<IResult> GetAllAccountsForUser(AccountGetAllForUserRequest accountGetAllForUserRequest);
    public Task<FirebaseObject<AccountData>?> GetAccountByUserIdAndBankCardNumber(string userId, string bankCardNumber);
    public Task<FirebaseObject<AccountData>?> FindAccountByAccountId(string accountId);

    public void Test();
}