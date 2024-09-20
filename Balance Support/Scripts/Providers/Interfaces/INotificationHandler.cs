using Balance_Support.DataClasses.Records.NotificationData;

namespace Balance_Support.Scripts.Providers.Interfaces;

public interface INotificationHandler
{
    public Task<IResult> HandleNotification(NotificationHandleRequest request);

    public void Test();
}