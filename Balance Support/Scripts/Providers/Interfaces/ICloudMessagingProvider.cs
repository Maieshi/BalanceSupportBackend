using Balance_Support.DataClasses.DatabaseEntities;
using Balance_Support.DataClasses.Records.NotificationData;

namespace Balance_Support.Scripts.Providers.Interfaces;

public interface ICloudMessagingProvider
{
    public Task<IResult> SetUserToken(SetUserTokenRequest request);

    public Task<IResult> DeleteUserToken(DeleteUserTokenRequest request);
    
    public Task<string> SendMessage(string userId, Account account, Transaction transactionData);

    public void Test();
}