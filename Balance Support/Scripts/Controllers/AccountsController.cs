using Balance_Support.DataClasses.DatabaseEntities;
using Balance_Support.DataClasses.Records.AccountData;
using Balance_Support.Scripts.Controllers.Interfaces;
using Balance_Support.Scripts.Database.Providers.Interfaces.Account;
using Balance_Support.Scripts.Database.Providers.Interfaces.User;
using Balance_Support.Scripts.Database.Providers.Interfaces.UserSettings;
using Balance_Support.Scripts.Main;

namespace Balance_Support.Scripts.Controllers;

public class AccountsController:IAccountsController
{
    public async Task<IResult> RegisterAccount(AccountRegisterRequest accountRegisterRequest,
        ICanProceedRequest canProceedRequest, IRegisterAccount registerAccount, ICheckUserWithIdExist idExist)
    {
        if (!await idExist.CheckId(accountRegisterRequest.UserId))
            return Results.NotFound("User");
        //TODO: check if account with same account number exists for this user
        if (!await canProceedRequest.CanProceedRequest(accountRegisterRequest.AccountData,
                accountRegisterRequest.UserId))
            return Results.Problem(statusCode: 500,
                title: "One account with same unique data already registered");
        try
        {
            var acc = await registerAccount.RegisterAccount(accountRegisterRequest);

            if (acc != null) return Results.Created("Accounts", new AccountDto(acc));

            return Results.Problem(statusCode: 500,
                title: "An error occurred while registering account");
        }
        catch (Exception ex)
        {
            return Results.Problem(ex.Message, statusCode: 500,
                title: "An error occurred while registering account");
        }
    }

    public async Task<IResult> UpdateAccount(AccountUpdateRequest accountUpdateRequest,
        IFindAccountByAccountId findAccountByAccountId, ICanProceedRequest canProceedRequest,
        IUpdateAccount updateAccount)
    {
        try
        {
            var account = await findAccountByAccountId.FindAccountByAccountId(accountUpdateRequest.AccountId);
            if (account == null)
                return Results.NotFound("Account");

            if (!await canProceedRequest.CanProceedRequest(accountUpdateRequest.AccountData,
                    accountUpdateRequest.UserId,
                    accountUpdateRequest.AccountId))
                return Results.Problem(statusCode: 500,
                    title: "One account with same unique data already registered");

            await updateAccount.UpdateAccount(account, accountUpdateRequest);
            return Results.Ok($"Accounts/{accountUpdateRequest.AccountId}");
        }
        catch (Exception ex)
        {
            return Results.Problem(ex.Message, statusCode: 500,
                title: "An error occurred while updating account");
        }
    }

    public async Task<IResult> DeleteAccount(AccountDeleteRequest accountDeleteRequest,
        IFindAccountByAccountId findAccountByAccountId, IDeleteAccount deleteAccount)
    {
        try
        {
            var currentAccount = await findAccountByAccountId.FindAccountByAccountId(accountDeleteRequest.AccountId);
            if (currentAccount == null)
                return Results.NotFound("Account");

            await deleteAccount.Delete(currentAccount);
            return Results.Ok($"Devices/{accountDeleteRequest.AccountId}");
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return Results.Problem(statusCode: 500, title: "Cannot delete account");
        }
    }

    public async Task<IResult> GetAccountsForDevice(AccountGetForDeviceRequest accountGetRequest,
        ICheckUserWithIdExist idExist, IFindAccountsByUserId findAccountsByUserId)
    {
        if (!await idExist.CheckId(accountGetRequest.UserId))
            return Results.NotFound("User");

        var accounts = (await findAccountsByUserId.FindAccountsByUserId(accountGetRequest.UserId))
            .Where(x =>
                x.AccountGroup == accountGetRequest.AccountGroup
                && x.DeviceId == accountGetRequest.DeviceId)
            .ToList();

        if (!accounts.Any())
            return Results.NotFound("Accounts");

        return Results.Ok(AccountDto.CreateDtos(accounts));
    }

    public async Task<IResult> GetAccountsForUser(AccountGetAllForUserRequest accountGetAllForUserRequest,
        ICheckUserWithIdExist idExist, IFindAccountsByUserId findAccountsByUserId,
        IGetUserSettingsByUserId getUserSettingsByUserId)
    {
        if (!await idExist.CheckId(accountGetAllForUserRequest.UserId))
            return Results.NotFound("User");

        var userSeetings = await getUserSettingsByUserId.GetByUserId(accountGetAllForUserRequest.UserId);
        if (userSeetings == null)
            return Results.NotFound("UserSettings");

        var accounts = await findAccountsByUserId.FindAccountsByUserId(accountGetAllForUserRequest.UserId);
        if (!accounts.Any())
            return Results.NotFound("Accounts");

        if (userSeetings.SelectedGroup!=0)
            accounts = accounts.Where(x => x.AccountGroup == userSeetings.SelectedGroup).ToList();
        if (!accounts.Any())
            return Results.NotFound("Accounts");

        var accountDtos = accounts.Select(x=>new  AccountDto(x)).ToList();       
        return Results.Ok(new
        {
            Accounts = accountDtos
        });
    }
}


