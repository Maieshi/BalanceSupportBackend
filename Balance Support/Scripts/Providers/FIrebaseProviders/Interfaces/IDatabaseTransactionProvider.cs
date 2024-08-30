namespace Balance_Support.Interfaces;

public interface IDatabaseTransactionProvider
{
    public  Task<IResult> RegisterNewTransaction(
        string userId,
        TransactionType transactionType,
        string cardNumber,
        decimal amount,
        decimal balance,
        string message
    );
}