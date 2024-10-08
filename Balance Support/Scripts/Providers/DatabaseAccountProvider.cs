using Balance_Support.DataClasses.DatabaseEntities;
using Balance_Support.DataClasses.Records.AccountData;
using Balance_Support.Scripts.Extensions.RecordExtenstions;
using Balance_Support.Scripts.Providers.Interfaces;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Balance_Support.Scripts.Providers;

public class DatabaseAccountProvider : IDatabaseAccountProvider
{
    private readonly ApplicationDbContext context;
    private readonly IDatabaseUserSettingsProvider userSettingsProvider;
    private readonly IDatabaseUserProvider userProvider;

    public DatabaseAccountProvider(IDatabaseUserProvider userProvider, ApplicationDbContext context, IDatabaseUserSettingsProvider userSettingsProvider)
    {
        this.userProvider = userProvider;
        this.context = context;
        this.userSettingsProvider = userSettingsProvider;
    }

    public async Task<IResult> RegisterAccount(AccountRegisterRequest accountRegisterRequest)
    {
        if (!await userProvider.IsUserWithIdExist(accountRegisterRequest.UserId))
            return Results.NotFound("User");
        //TODO: check if account with same account number exists for this user
        if (!await CanProceedRequest(accountRegisterRequest.AccountData,accountRegisterRequest.UserId))
            return Results.Problem(statusCode: 500,
                title: "One account with same unique data already registered");
        try
        {
            var acc = context.Accounts.Add(accountRegisterRequest.NewAccount());
            await context.SaveChangesAsync();

            return Results.Created("Accounts", new AccountDto(acc.Entity));
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
                return Results.NotFound("Account");
            
            if (!await CanProceedRequest(accountUpdateRequest.AccountData,accountUpdateRequest.UserId, accountUpdateRequest.AccountId))
                return Results.Problem(statusCode: 500,
                    title: "One account with same unique data already registered");

            account.UpdateAccount(accountUpdateRequest);
            context.Accounts.Update(account);
            await context.SaveChangesAsync();

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
            var currentAccount = await FindAccountByAccountId(accountDeleteRequest.AccountId);
            if (currentAccount == null)
                return Results.NotFound("Account");

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
            return Results.NotFound("User");

        var accounts = (await FindAccountsByUserId(accountGetRequest.UserId))
            .Where(x =>
                x.AccountGroup == accountGetRequest.AccountGroup
                && x.DeviceId == accountGetRequest.DeviceId)
            .ToList();

        if (!accounts.Any())
            return Results.NotFound("Accounts");

        return Results.Ok(AccountDto.CreateDtos(accounts));
    }

    public async Task<IResult> GetAllAccountsForUser(AccountGetAllForUserRequest accountGetAllForUserRequest)
    {
        if (!await userProvider.IsUserWithIdExist(accountGetAllForUserRequest.UserId))
            return Results.NotFound("User");

        var userSeetings = await userSettingsProvider.GetUserSettings(accountGetAllForUserRequest.UserId);
        if(userSeetings==null)
            return Results.NotFound("UserSettings");

        var accounts = await FindAccountsByUserId(accountGetAllForUserRequest.UserId);
        if (!accounts.Any())
            return Results.NotFound("Accounts");
        
        if(userSeetings.SelectedGroup!=0)
            accounts = accounts.Where(x => x.AccountGroup == userSeetings.SelectedGroup).ToList();
        
        var globalIncome =await CalculateGlobalIncome(accountGetAllForUserRequest.UserId);
        
        var accountDtos = new List<object>();
        foreach (var account in accounts)
        {
            var accountIncome = await CalculateIncomeForAccount(account.Id);
            accountDtos.Add(new
            {
                Account = new AccountDto(account),
                T =accountIncome.total,
                D = accountIncome.daily
            });
        }

        return Results.Ok(new
        {
            Balance = globalIncome.total,
            DailyExpression = globalIncome.daily,
            Accounts =  accountDtos
        });
    }

    public async Task<Account?> GetAccountByUserIdAndAccountNumber(string userId, string accountNumber)
    {
        return await context.Accounts.Where(x => x.UserId == userId && x.AccountNumber == accountNumber)
            .FirstOrDefaultAsync();
    }


    public async Task<Account?> GetAccountByUserIdAndBankCardNumber(string userId,
        string bankCardNumber)
    {
        return (await FindAccountsByUserId(userId))
            .FirstOrDefault(x =>
                string.Equals(x.BankCardNumber, bankCardNumber));
    }

    public async Task<Account?> FindAccountByAccountId(string accountId)
    {
        return await context.Accounts.FindAsync(accountId);
    }

    private async Task<List<Account>> FindAccountsByUserId(string userId)
    {
        return await context.Accounts.Where(x => x.UserId == userId)
            .ToListAsync();
    } 


      
    
    private async Task<bool> CanProceedRequest(AccountDataRequest accountData, string userId, string? accountId = null)
    {
        var existingAccounts = await context.Accounts
            .Where(x => x.UserId == userId && (accountId == null || x.Id != accountId))
            .ToListAsync();

        var hasSameAccountNumber = existingAccounts.Exists(x => x.AccountNumber == accountData.AccountNumber);
        var hasSameSimCardNumber = existingAccounts.Exists(x => x.SimCardNumber == accountData.SimCardNumber);
        var hasSameGroupDeviceSlot = existingAccounts.Exists(x =>
            x.AccountGroup == accountData.AccountGroup &&
            x.DeviceId == accountData.DeviceId &&
            x.SimSlot == accountData.SimSlot);

        return !(hasSameAccountNumber || hasSameSimCardNumber || hasSameGroupDeviceSlot);
    }
    
    public async Task<(float total, float daily)> CalculateIncomeForAccount(string accountId)
    {
        var transactions = await context.Transactions.Where(x => accountId == x.AccountId).ToListAsync();
        
        float totalIncome = (float)transactions.Sum(x => x.Amount);
        float dailyIncome = (float)transactions
            .Where(x => x.Time.Date == DateTime.UtcNow.Date)
            .Sum(x => x.Amount);
        return (totalIncome, dailyIncome);
    }
    
    public async Task<(float total, float daily)> CalculateGlobalIncome(string userId)
    {
        var transactions = await context.Transactions.Where(x => userId == x.UserId).ToListAsync();
        
        float totalIncome = (float)transactions.Sum(x => x.Amount);
        float dailyIncome = (float)transactions
            .Where(x => x.Time.Date == DateTime.UtcNow.Date)
            .Sum(x => x.Amount);
        return (totalIncome, dailyIncome);
    }
    
}