using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;

namespace Balance_Support
{
    public class CloudMessagingProvider
    {

        public CloudMessagingProvider() {
            
            var message = new Message()
            {
                Notification = new Notification
                {
                    Title = "Message Title",
                    Body = "Message Body"
                },
                Data = new Dictionary<string, string>()
                {
                    ["CustomData"] = "Custom Data"
                },
                Token = "TARGET_DEVICE_TOKEN" // Replace with the actual device token of the target device
            };

            FirebaseMessaging.DefaultInstance.SendAsync(message);
        }
    }
}
