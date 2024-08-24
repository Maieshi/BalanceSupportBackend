using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;

namespace Balance_Support
{
    public class CloudMessagingProvider
    {

        public CloudMessagingProvider() {
            
            
        }

        public async void Test()
        {
            
            string token = "fCR2itQ0_EWyYk-YueXpW1:APA91bHlwFa_vq6YEOf7SvRDklt5Nso_-X4Hx8Iz-GZU1z0BqliRIDpdi4Ru0aqDJJ9G0qlQtlYCnbQj2evs6wcMpLr_RFc-_ukud1qe0qvxlzhDPQPOrfWOIZrPFXozGj0Z8UPCqbLX";
            var message = new Message()
            {
                Data = new Dictionary<string, string>()
                {
                    { "title", "Hello!" },
                    { "body", "This is a test message." }
                },
                Token = token
            };

            // Send the message
            string response = await FirebaseMessaging.DefaultInstance.SendAsync(message);

            // Log the response
            Console.WriteLine("Successfully sent message: " + response);
            
            
            var message1 = new Message()
            {
                
                Data = new Dictionary<string, string>()
                {
                    ["CustomData"] = "Custom Data",
                    ["Greeting"] = "Hello, Melo1man!"
                },
                Token = "APA91bG35mtrs9Wz_iXheMj7M8p4xfohA35Kkpk4AkZIPoSaiOKtxrDotc7xAg3Qc3m3mWxH1H_ofsoEDnf_8E95BLI5ORbrfeahGGPmKHmeZlyGo7mhN3r_Mp9ueNzRSjeVXr5S_G_D\n" // Replace with the actual device token of the target device
            };

            var response1 = await FirebaseMessaging.DefaultInstance.SendAsync(message);
            
            var message2 = new Message()
            {
                Data = new Dictionary<string, string>()
                {
                    ["accountID"] = "1233",
                    ["Name"] = "Matveychik",
                    ["Balance"] = "3000.10",
                    ["Group"] = "3",
                    ["Device"] = "3.2",
                    ["Slim sim"] = "2",
                    ["Phone"] = "12345678901",
                    ["Card "] = "5000,6543",
                    ["Bank "] = "Sberbank",
                    ["Expenses t "] = "12345 123,12",
                    ["Expenses D "] = "12345 123,120",
                    ["tIME "] = "02.01.2024 11:50",
                    ["Info "] = "AAAA",
                },
                Token = "APA91bG35mtrs9Wz_iXheMj7M8p4xfohA35Kkpk4AkZIPoSaiOKtxrDotc7xAg3Qc3m3mWxH1H_ofsoEDnf_8E95BLI5ORbrfeahGGPmKHmeZlyGo7mhN3r_Mp9ueNzRSjeVXr5S_G_D\n" // Replace with the actual device token of the target device
            };
        //     var response2=await FirebaseMessaging.DefaultInstance.SendAsync(message);
        }
        
    }
}
