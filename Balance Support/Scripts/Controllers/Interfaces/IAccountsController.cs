using Balance_Support.DataClasses.Records.AccountData;
using Balance_Support.DataClasses.Validators;
using Balance_Support.Scripts.Database.Providers.Interfaces.Account;
using Balance_Support.Scripts.Database.Providers.Interfaces.User;
using Balance_Support.Scripts.Database.Providers.Interfaces.UserSettings;

namespace Balance_Support.Scripts.Controllers.Interfaces;

public interface IAccountsController
{
    public Task<IResult> RegisterAccount(AccountRegisterRequest accountRegisterRequest);

    public Task<IResult> UpdateAccount(AccountUpdateRequest accountUpdateRequest);

    public Task<IResult> DeleteAccount(AccountDeleteRequest accountDeleteRequest);
    
    public Task<IResult> SetAccountBalance(AccountSetBalanceRequest accountSetBalanceRequest);

    public Task<IResult> GetAccountsForDevice(AccountGetForDeviceRequest accountGetRequest);

    public Task<IResult> GetAllAccountsForUser(AccountGetAllForUserRequest accountGetAllForUserRequest);
    
    public Task<IResult> GetAllAccountGroupsForUser(AccountGetAllGroupsForUserRequest accountGetAllGroupsForUserRequest);
    public Task<IResult> GetAllAccountNumbersForUser(AccountGetAllAccountNumbersForUserRequest accountGetAllGroupsForUserRequest);
}