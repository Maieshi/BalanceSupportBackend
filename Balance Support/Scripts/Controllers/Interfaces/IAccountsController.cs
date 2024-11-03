using Balance_Support.DataClasses.Records.AccountData;
using Balance_Support.Scripts.Database.Providers.Interfaces.Account;
using Balance_Support.Scripts.Database.Providers.Interfaces.User;
using Balance_Support.Scripts.Database.Providers.Interfaces.UserSettings;

namespace Balance_Support.Scripts.Controllers.Interfaces;

public interface IAccountsController
{
    public Task<IResult> RegisterAccount(AccountRegisterRequest accountRegisterRequest,
        ICanProceedRequest canProceedRequest, IRegisterAccount registerAccount, ICheckUserWithIdExist idExist);

    public Task<IResult> UpdateAccount(AccountUpdateRequest accountUpdateRequest,
        IFindAccountByAccountId findAccountByAccountId, ICanProceedRequest canProceedRequest,
        IUpdateAccount updateAccount);

    public Task<IResult> DeleteAccount(AccountDeleteRequest accountDeleteRequest,
        IFindAccountByAccountId findAccountByAccountId, IDeleteAccount deleteAccount);
    
    public Task<IResult> SetAccountBalance(AccountSetBalanceRequest accountSetBalanceRequest,
        IFindAccountByAccountId findAccountByAccountId, IUpdateAccount updateAccount);

    public Task<IResult> GetAccountsForDevice(AccountGetForDeviceRequest accountGetRequest,
        ICheckUserWithIdExist idExist, IFindAccountsByUserId findAccountsByUserId);

    public Task<IResult> GetAllAccountsForUser(AccountGetAllForUserRequest accountGetAllForUserRequest,
        ICheckUserWithIdExist idExist, IFindAccountsByUserId findAccountsByUserId);
}