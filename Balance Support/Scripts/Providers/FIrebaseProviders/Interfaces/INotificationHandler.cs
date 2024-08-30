using Balance_Support.DataClasses.Records.NotificationData;

namespace Balance_Support.Interfaces;

public interface INotificationHandler
{
    public Task<IResult> HandleNotification(NotificationHandleRequest request);

    public void Test();
}