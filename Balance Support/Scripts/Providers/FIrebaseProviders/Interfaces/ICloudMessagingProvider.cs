using Balance_Support.DataClasses.Records.AccountData;
using Balance_Support.DataClasses.Records.NotificationData.DatabaseInfo;
using Balance_Support.DataClasses.Records.NotificationData;
using Balance_Support.DataClasses.DatabaseEntities;
namespace Balance_Support.Interfaces;

public interface ICloudMessagingProvider
{
    public Task<IResult> RegisterUserToken(UserTokenRequest request);
    public Task<IResult> UpdateUserToken(UserTokenRequest request);
    public Task<string> SendMessage(string userId, Account account, TransactionData transactionData);

    public void Test();
}