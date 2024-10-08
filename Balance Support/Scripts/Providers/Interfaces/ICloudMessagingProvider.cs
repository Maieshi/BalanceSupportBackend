using Balance_Support.DataClasses.DatabaseEntities;
using Balance_Support.DataClasses.Records.NotificationData;

namespace Balance_Support.Scripts.Providers.Interfaces;

public interface ICloudMessagingProvider
{
    public Task<IResult> SetUserToken(SetUserTokenRequest request);

    public Task<IResult> DeleteUserToken(DeleteUserTokenRequest request);
    
    public Task<string> SendTransaction(string userId,string accountID, float accountTotal, float accountDaily, float balance, float dailyExpression);

    public Task<IResult> SendMessages(string userId, List<Transaction> transactions, List<Account> accounts);

    public void Test();
}