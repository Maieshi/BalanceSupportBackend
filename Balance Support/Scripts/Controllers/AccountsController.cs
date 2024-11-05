using Balance_Support.DataClasses.DatabaseEntities;
using Balance_Support.DataClasses.Records.AccountData;
using Balance_Support.Scripts.Controllers.Interfaces;
using Balance_Support.Scripts.Database;
using Balance_Support.Scripts.Database.Providers.Interfaces.Account;
using Balance_Support.Scripts.Database.Providers.Interfaces.User;
using Balance_Support.Scripts.Database.Providers.Interfaces.UserSettings;
using Balance_Support.Scripts.Main;

namespace Balance_Support.Scripts.Controllers;

public class AccountsController:IAccountsController
{
    private readonly IDatabaseAccountProvider accounts;
    private readonly IDatabaseUserProvider users;

    public AccountsController(IDatabaseAccountProvider accounts,
        IDatabaseUserProvider users)
    {
        this.accounts = accounts;
        this.users = users;
    }
    public async Task<IResult> RegisterAccount(AccountRegisterRequest accountRegisterRequest)
    {
        if (!await users.CheckUserWithIdExist(accountRegisterRequest.UserId))
            return Results.NotFound("User");
        //TODO: check if account with same account number exists for this user
        if (!await accounts.CanProceedRequest(accountRegisterRequest.AccountData,
                accountRegisterRequest.UserId))
            return Results.Problem(statusCode: 500,
                title: "One account with same unique data already registered");
        try
        {
            var acc = await accounts.RegisterAccount(accountRegisterRequest);

            if (acc != null) return Results.Created("Accounts", acc.Convert());

            return Results.Problem(statusCode: 500,
                title: "An error occurred while registering account");
        }
        catch (Exception ex)
        {
            return Results.Problem(ex.Message, statusCode: 500,
                title: "An error occurred while registering account");
        }
    }

    public async Task<IResult> UpdateAccount(AccountUpdateRequest accountUpdateRequest)
    {
        try
        {
            var account = await accounts.FindAccountByAccountId(accountUpdateRequest.AccountId);
            if (account == null)
                return Results.NotFound("Account");

            if (!await accounts.CanProceedRequest(accountUpdateRequest.AccountData,
                    accountUpdateRequest.UserId,
                    accountUpdateRequest.AccountId))
                return Results.Problem(statusCode: 500,
                    title: "One account with same unique data already registered");

            await accounts.UpdateAccount(account, accountUpdateRequest);
            return Results.Ok($"Accounts/{accountUpdateRequest.AccountId}");
        }
        catch (Exception ex)
        {
            return Results.Problem(ex.Message, statusCode: 500,
                title: "An error occurred while updating account");
        }
    }

    public async Task<IResult> DeleteAccount(AccountDeleteRequest accountDeleteRequest)
    {
        try
        {
            var currentAccount = await accounts.FindAccountByAccountId(accountDeleteRequest.AccountId);
            if (currentAccount == null)
                return Results.NotFound("Account");

            await accounts.Delete(currentAccount);
            return Results.Ok($"Devices/{accountDeleteRequest.AccountId}");
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return Results.Problem(statusCode: 500, title: "Cannot delete account");
        }
    }

    public async Task<IResult> SetAccountBalance(AccountSetBalanceRequest accountSetBalanceRequest)
    {
        var account = await accounts.FindAccountByAccountId(accountSetBalanceRequest.AccountId);
        if(account==null) return Results.NotFound("Account");       
        account.SmsBalance = accountSetBalanceRequest.Balance;
        accounts.UpdateAccount(account);
        return Results.Ok("Balance updated");
    }

    public async Task<IResult> GetAccountsForDevice(AccountGetForDeviceRequest accountGetRequest)
    {
        if (!await users.CheckUserWithIdExist(accountGetRequest.UserId))
            return Results.NotFound("User");

        var accountsFound = (await accounts.FindAccountsByUserId(accountGetRequest.UserId))
            .Where(x =>
                x.AccountGroup == accountGetRequest.AccountGroup
                && x.DeviceId == accountGetRequest.DeviceId)
            .ToList();

        if (!accountsFound.Any())
            return Results.NotFound("Accounts");

        return Results.Ok(accountsFound.ConvertToDtoList());
    }

    public async Task<IResult> GetAllAccountsForUser(AccountGetAllForUserRequest accountGetAllForUserRequest)
    {
        if (!await users.CheckUserWithIdExist(accountGetAllForUserRequest.UserId))
            return Results.NotFound("User");
        
        var accountsFound = await accounts.FindAccountsByUserId(accountGetAllForUserRequest.UserId);
        if (!accountsFound.Any())
            return Results.NotFound("Accounts");
        
               
        return Results.Ok(new
        {
            Accounts = accountsFound.ConvertToDtoList()
        });
    }
}


