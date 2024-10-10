using Balance_Support.DataClasses.Records.AccountData;

namespace Balance_Support.Scripts.Database.Providers.Interfaces.Account;

public interface ICanProceedRequest
{
    public Task<bool> CanProceedRequest(AccountDataRequest accountData, string userId, string? accountId = null);
}