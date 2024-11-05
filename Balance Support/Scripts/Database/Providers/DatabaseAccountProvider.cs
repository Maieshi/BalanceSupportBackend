using Balance_Support.DataClasses.DatabaseEntities;
using Balance_Support.DataClasses.Records.AccountData;
using Balance_Support.Scripts.Database.Providers.Interfaces.Account;
using Balance_Support.Scripts.Extensions.RecordExtenstions;
using Microsoft.EntityFrameworkCore;

namespace Balance_Support.Scripts.Database.Providers;

public class DatabaseAccountProvider : DbSetController<Account>,IDatabaseAccountProvider
{
    public DatabaseAccountProvider(IDbSetContainer container, ISaveDbChanges saver) : base(container, saver)
    {
    }

    public async Task<Account?> RegisterAccount(AccountRegisterRequest accountRegisterRequest)
    {
        var acc = Table.Add(accountRegisterRequest.NewAccount());
        await Saver.SaveChangesAsync();
        return acc.Entity;
    }


    public async Task UpdateAccount(Account account, AccountUpdateRequest accountUpdateRequest)
    {
        account.UpdateAccount(accountUpdateRequest);
        Table.Update(account);
        await Saver.SaveChangesAsync();
    }

    public async Task UpdateAccount(Account account)
    {
        Table.Update(account);
        await Saver.SaveChangesAsync();
    }

    public async Task Delete(Account account)
    {
        Table.Remove(account);
        await Saver.SaveChangesAsync();
    }


    public async Task<List<Account>> GetAccoutForDeivce(string userId, int accountGroup, int deviceId)
        =>
            (await FindAccountsByUserId(userId))
            .Where(x =>
                x.AccountGroup == accountGroup
                && x.DeviceId == deviceId)
            .ToList();


    public async Task<List<Account>> GetAllAccountsForUser(string userId, int? selectedGroup = null) =>
        await FindAccountsByUserId(userId)
            .ContinueWith(t => t.Result
                .Where(x => !selectedGroup.HasValue || x.AccountGroup == selectedGroup.Value)
                .ToList()
            );

    public async Task<Account?> GetAccountByUserIdAndAccountNumber(string userId, string accountNumber)
    {
        return await Table.Where(x => x.UserId == userId && x.AccountNumber == accountNumber)
            .FirstOrDefaultAsync();
    }

    public async Task<Account?> GetAccountByUserIdAndBankCardNumber(string userId, string bankCardNumber)
    {
        return (await FindAccountsByUserId(userId))
            .FirstOrDefault(x =>
                string.Equals(x.BankCardNumber, bankCardNumber));
    }

    public async Task<Account?> FindAccountByAccountId(string accountId)
    {
        return await Table.FindAsync(accountId);
    }

    public async Task<List<Account>> FindAccountsByUserId(string userId)
    {
        return await Table.Where(x => x.UserId == userId)
            .ToListAsync();
    }

    public async Task<bool> CanProceedRequest(AccountDataRequest accountData, string userId, string? accountId = null)
    {
        var existingAccounts = await Table
            .Where(x => x.UserId == userId && (accountId == null || x.Id != accountId))
            .ToListAsync();

        var hasSameAccountNumber = existingAccounts.Exists(x => x.AccountNumber == accountData.AccountNumber);
        var hasSameSimCardNumber = existingAccounts.Exists(x => x.SimCardNumber == accountData.SimCardNumber);
        var hasSameGroupDeviceSlot = existingAccounts.Exists(x =>
            x.AccountGroup == accountData.AccountGroup &&
            x.DeviceId == accountData.DeviceId &&
            x.SimSlot == accountData.SimSlot);
        var hasSameBankCardNumberInGroup = existingAccounts.Exists(x =>
            x.AccountGroup == accountData.AccountGroup &&
            x.BankCardNumber == accountData.BankCardNumber);

        return !(hasSameAccountNumber || hasSameSimCardNumber || hasSameGroupDeviceSlot ||
                 hasSameBankCardNumberInGroup);
    }
}