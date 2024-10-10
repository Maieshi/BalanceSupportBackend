namespace Balance_Support.Scripts.Database.Providers.Interfaces.Transaction;

public interface IGetTransactionsForAccount
{
    public Task<List<DataClasses.DatabaseEntities.Transaction>> Get(string accountId);
}