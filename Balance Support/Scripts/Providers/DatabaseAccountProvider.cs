// using Firebase.Auth;

using Balance_Support.DataClasses.DatabaseEntities;
using Balance_Support.DataClasses.Records.AccountData;
using Balance_Support.Scripts.Extensions.RecordExtenstions;
using Balance_Support.Scripts.Providers.Interfaces;
using Microsoft.EntityFrameworkCore;

// using FireSharp;
// using FireSharp.Interfaces;
// using FireSharp.Response;
// using FireSharp.Config;
// using Google.Apis.Auth.OAuth2;

namespace Balance_Support.Scripts.Providers;

public class DatabaseAccountProvider : IDatabaseAccountProvider
{
    private readonly ApplicationDbContext context;
    private readonly IDatabaseUserProvider userProvider;

    public DatabaseAccountProvider(IDatabaseUserProvider userProvider, ApplicationDbContext context)
    {
        this.userProvider = userProvider;
        this.context = context;
    }

    public async Task<IResult> RegisterAccount(AccountRegisterRequest accountRegisterRequest)
    {
        if (!await userProvider.IsUserWithIdExist(accountRegisterRequest.UserId))
            return Results.Problem(statusCode: 500, title: "User not found");


        if (await IsAlreadyExistAccountWithGropAndDeviceId(accountRegisterRequest.UserId,
                accountRegisterRequest.AccountData.AccountGroup, accountRegisterRequest.AccountData.DeviceId,
                accountRegisterRequest.AccountData.SimSlot))
            return Results.Problem(statusCode: 500,
                title: "One account with same group and device id already registered");

        try
        {
            var acc =  context.Accounts.Add(accountRegisterRequest.NewAccount());
            await context.SaveChangesAsync();
            
            
            return Results.Created("Accounts", acc.Entity);
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
            var account = await FindAccountByAccountId(accountUpdateRequest.AccountId);
            if (account == null)
                return Results.Problem(statusCode: 500, title: "Account not found");

            if (await IsAlreadyExistAccountWithGropAndDeviceId(accountUpdateRequest.UserId,
                    accountUpdateRequest.AccountDataRequest.AccountGroup,
                    accountUpdateRequest.AccountDataRequest.DeviceId, accountUpdateRequest.AccountDataRequest.SimSlot))
                return Results.Problem(statusCode: 500,
                    title: "One account with same group and device id already registered");

            account.UpdateAccount(accountUpdateRequest);
            context.Accounts.Update(account);
            await context.SaveChangesAsync();

            return Results.Ok($"Accounts/{accountUpdateRequest.AccountId}");
        }
        catch (Exception ex)
        {
            return Results.Problem(ex.Message, statusCode: 500,
                title: "An error occurred while updating device ");
        }
    }


    public async Task<IResult> DeleteDevice(AccountDeleteRequest accountDeleteRequest)
    {
        try
        {
            var currentAccount = await FindAccountByAccountId(accountDeleteRequest.AccountId);
            if (currentAccount == null)
                return Results.Problem(statusCode: 500, title: "Account not found");

           context.Accounts.Remove(currentAccount);
            await context.SaveChangesAsync();
            return Results.Ok($"Devices/{accountDeleteRequest.AccountId}");
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return Results.Problem(statusCode: 500, title: "Cannot delete account");
        }
    }

    public async Task<IResult> GetAccountsForDevice(AccountGetForDeviceRequest accountGetRequest)
    {
        if (!await userProvider.IsUserWithIdExist(accountGetRequest.UserId))
            return Results.Problem(statusCode: 500, title: "User not found");

        var accounts = (await FindAccountsByUserId(accountGetRequest.UserId))
            .Where(x =>
                x.AccountGroup == accountGetRequest.AccountGroup
                && x.DeviceId == accountGetRequest.DeviceId)
            .ToList();

        if (accounts.Any())
            return Results.Problem(statusCode: 500, title: "Accounts not found");
        
        return Results.Ok(new
        {
            Accounts = accounts
        });
    }
    
    public async Task<IResult> GetAllAccountsForUser(AccountGetAllForUserRequest accountGetAllForUserRequest)
    {
        if (!await userProvider.IsUserWithIdExist(accountGetAllForUserRequest.UserId))
            return Results.Problem(statusCode: 500, title: "User not found");
        
        var accounts = await FindAccountsByUserId(accountGetAllForUserRequest.UserId);
        if (accounts.Any())
            return Results.Problem(statusCode: 500, title: "Accounts not found");
        
        return Results.Ok(new
        {
            Accounts = accounts
        });
    }

    public async void Test()
    {
        var result = await RegisterAccount(
            new AccountRegisterRequest(
                "sDAmWae7RqMsmWIC74lVdLuQRpq1",
                new AccountDataRequest(
                    "123456789",
                    "Ivaniv",
                    3,
                    3,
                    1,
                    "+88005553535",
                    "1488",
                    "SberBank",
                    "Very rich person"
                )));
    }

    public async Task<Account?> GetAccountByUserIdAndBankCardNumber(string userId,
        string bankCardNumber)
     =>(await FindAccountsByUserId(userId))
            .FirstOrDefault(x =>
                string.Equals(x.BankCardNumber, bankCardNumber));
    
    public async Task<Account?> FindAccountByAccountId(string accountId)
        => await context.Accounts.FindAsync(accountId);

    private async Task<List<Account>> FindAccountsByUserId(string userId)
        => await context.Accounts.Where(x => x.UserId == userId)
            .ToListAsync();

    private async Task<bool> IsAlreadyExistAccountWithGropAndDeviceId(string userId, int accountGroup,
        int deviceId, int simSlot)
        =>
            (await context.Accounts.Where(acc =>
                    acc.UserId == userId &&
                    acc.AccountGroup == accountGroup &&
                    acc.DeviceId == deviceId &&
                    acc.SimSlot == simSlot)
                .FirstOrDefaultAsync()) != null;

}