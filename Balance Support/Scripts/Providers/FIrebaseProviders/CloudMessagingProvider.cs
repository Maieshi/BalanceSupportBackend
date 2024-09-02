using Balance_Support.DataClasses.Records.AccountData;
using Balance_Support.DataClasses.Records.NotificationData;
using Balance_Support.DataClasses.Records.NotificationData.DatabaseInfo;
using Balance_Support.Interfaces;
using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Firebase.Database;
using Firebase.Database.Query;
namespace Balance_Support
{
    public class CloudMessagingProvider:ICloudMessagingProvider
    {
        private readonly FirebaseClient client;

        public CloudMessagingProvider(FirebaseClient client)
        {
            this.client = client;
        }

        public async Task<IResult> RegisterUserToken(UserTokenRequest request)
        {
            try
            {
                var tokenResult = await FindUser(request.UserId);
                
                if (tokenResult==null||tokenResult.Any())
                {
                    return Results.Problem(statusCode: 500, title: "Token for this user  already registered");
                }
                return Results.Problem(statusCode: 500, title: "Token for this user  already registered");
                
                var result = await client
                    .Child("UserTokens")
                    .PostAsync(request);
                
                return Results.Ok();
            }
            catch (Exception e)
            {
                return Results.Problem(statusCode: 500, title: "Token registration error");
            }
        }
        
        public async Task<IResult> UpdateUserToken(UserTokenRequest request)
        {
            try
            {
                var tokenResult = await FindUser(request.UserId);
                
                var key = tokenResult?.FirstOrDefault()?.Key;
                
                if (string.IsNullOrEmpty(key))
                {
                    return Results.Problem(statusCode: 500, title: "User not found");
                }
                
                await client
                    .Child("UserTokens")
                    .Child(key)
                    .PutAsync(request);
                
                return Results.Ok(key);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
        
        private async Task<IReadOnlyCollection<FirebaseObject<UserTokenRequest>>?> FindUser(string userId)
            => await client
            .Child("UserTokens")
            .OrderBy("UserId")
            .EqualTo(userId)
            .OnceAsync<UserTokenRequest>();
        
        public async Task<string> SendMessage(string userId,AccountData account, TransactionData transactionData)
        {
            var user = (await FindUser(userId))?.SingleOrDefault();
            
            if (user == null)return null;
            
            var message2 = new Message()
            {
                Data = new Dictionary<string, string>()
                {
                    ["accountID"] = account.AccountNumber,
                    ["Name"] = account.LastName,
                    ["Balance"] =transactionData.Balance.ToString(),
                    ["Group"] = account.AccountGroup.ToString(),
                    ["Device"] = account.DeviceId.ToString(),
                    ["Sim slot"] = account.SimSlot.ToString(),
                    ["Phone"] = account.SimCardNumber,
                    ["Card "] = account.BankCardNumber,
                    ["Bank "] = account.BankType.ToString(),
                    ["Expenses t "] = "12345 123,12",
                    ["Expenses D "] = "12345 123,120",
                    ["tIME "] = "02.01.2024 11:50",
                    ["Info "] = transactionData.Message,
                },
                Token = user.Object.Token
            };
            
           var res=  await FirebaseMessaging.DefaultInstance.SendAsync(message2);

           return res;
        }

        public async void Test()
        {
          var a =   await RegisterUserToken(
                    new UserTokenRequest(
                        "sDAmWae7RqMsmWIC74lVdLuQRpq1",
                        "cTJewO0m_f_-6jVyEowMES:APA91bEt7ix5HNm-ct42Hoc3fJC1aTCkDVoPg7952GndQm2BEJushDRtAzaCjZXtjO8olOZNz__3EUFCMPuuYyPTT9StBTVrFe5yZDBnOdxLy0n9xIbImt5qsphtVRXkmb2BQu0LgvQ-"));
            
          
            // string token = "fCR2itQ0_EWyYk-YueXpW1:APA91bHlwFa_vq6YEOf7SvRDklt5Nso_-X4Hx8Iz-GZU1z0BqliRIDpdi4Ru0aqDJJ9G0qlQtlYCnbQj2evs6wcMpLr_RFc-_ukud1qe0qvxlzhDPQPOrfWOIZrPFXozGj0Z8UPCqbLX";
            // var message = new Message()
            // {
            //     Data = new Dictionary<string, string>()
            //     {
            //         { "title", "Hello!" },
            //         { "body", "This is a test message." }
            //     },
            //     Token = token
            // };
            //
            // // Send the message
            // string response = await FirebaseMessaging.DefaultInstance.SendAsync(message);
            //
            // // Log the response
            // Console.WriteLine("Successfully sent message: " + response);
            //
            //
            // var message1 = new Message()
            // {
            //     Data = new Dictionary<string, string>()
            //     {
            //         ["CustomData"] = "Custom Data",
            //         ["Greeting"] = "Hello, Melo1man!"
            //     },
            //     Token = "APA91bG35mtrs9Wz_iXheMj7M8p4xfohA35Kkpk4AkZIPoSaiOKtxrDotc7xAg3Qc3m3mWxH1H_ofsoEDnf_8E95BLI5ORbrfeahGGPmKHmeZlyGo7mhN3r_Mp9ueNzRSjeVXr5S_G_D\n" // Replace with the actual device token of the target device
            // };
            //
            //
            //
            // var message2 = new Message()
            // {
            //     Data = new Dictionary<string, string>()
            //     {
            //         ["accountID"] = "1233",
            //         ["Name"] = "Matveychik",
            //         ["Balance"] = "3000.10",
            //         ["Group"] = "3",
            //         ["Device"] = "3.2",
            //         ["Sim slot"] = "2",
            //         ["Phone"] = "12345678901",
            //         ["Card "] = "5000,6543",
            //         ["Bank "] = "Sberbank",
            //         ["Expenses t "] = "12345 123,12",
            //         ["Expenses D "] = "12345 123,120",
            //         ["tIME "] = "02.01.2024 11:50",
            //         ["Info "] = "AAAA",
            //     },
            //     Token = "APA91bG35mtrs9Wz_iXheMj7M8p4xfohA35Kkpk4AkZIPoSaiOKtxrDotc7xAg3Qc3m3mWxH1H_ofsoEDnf_8E95BLI5ORbrfeahGGPmKHmeZlyGo7mhN3r_Mp9ueNzRSjeVXr5S_G_D\n" // Replace with the actual device token of the target device
            // };
        //     var response2=await FirebaseMessaging.DefaultInstance.SendAsync(message);
        }
    }

    
}
