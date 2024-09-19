using Balance_Support.DataClasses;
using Balance_Support.DataClasses.Records.AccountData;
using Balance_Support.DataClasses.Records.NotificationData.DatabaseInfo;
using Balance_Support.DataClasses.Records.NotificationData;
using Balance_Support.DataClasses.DatabaseEntities;
namespace Balance_Support.Interfaces;

public interface ICloudMessagingProvider
{
    public Task<IResult> SetUserToken(SetUserTokenRequest request);

    public Task<IResult> DeleteUserToken(DeleteUserTokenRequest request);
    
    public Task<string> SendMessage(string userId, Account account, Transaction transactionData);

    public void Test();
}