using Balance_Support.DataClasses.DatabaseEntities;
using Balance_Support.DataClasses.Records.NotificationData;
using Balance_Support.Interfaces;
using Firebase.Database;
using FirebaseAdmin.Messaging;
using Microsoft.EntityFrameworkCore;

namespace Balance_Support;

public class CloudMessagingProvider : ICloudMessagingProvider
{
    private readonly FirebaseClient client;
    private readonly ApplicationDbContext context;

    public CloudMessagingProvider(FirebaseClient client, ApplicationDbContext context)
    {
        this.client = client;
        this.context = context;
    }

    public async Task<IResult> SetUserToken(SetUserTokenRequest request)
    {
        try
        {
            var tokenResult = await FindUserToken(request.UserId);
            
            var action = "update";
            
            if (tokenResult == null)
            {
                action = "insert";
                tokenResult = new UserToken();
            }
            else
            {
                tokenResult.Token = request.Token;
            }

            if (tokenResult != null)
                tokenResult.Token = request.Token;
            else
                context.UserTokens.Add(new UserToken
                {
                    Id = Guid.NewGuid().ToString(),
                    UserId = request.UserId,
                    Token = request.Token
                });

            await context.SaveChangesAsync();


            return Results.Ok($"Token {action}");
        }
        catch (Exception e)
        {
            return Results.Problem(statusCode: 500, title: "Token registration error");
        }
    }

    public async Task<string> SendMessage(string userId, Account account, Transaction transactionData)
    {
        var user = await FindUserToken(userId);

        if (user == null) return null;

        var message2 = new Message
        {
            Data = new Dictionary<string, string>
            {
                ["accountID"] = account.AccountNumber,
                ["Name"] = account.LastName,
                ["Balance"] = transactionData.Balance.ToString(),
                ["Group"] = account.AccountGroup.ToString(),
                ["Device"] = account.DeviceId.ToString(),
                ["Sim slot"] = account.SimSlot.ToString(),
                ["Phone"] = account.SimCardNumber,
                ["Card "] = account.BankCardNumber,
                ["Bank "] = account.BankType,
                ["Expenses t "] = "12345 123,12",
                ["Expenses D "] = "12345 123,120",
                ["tIME "] = "02.01.2024 11:50",
                ["Info "] = transactionData.Message
            },
            Token = user.Token
        };

        var res = await FirebaseMessaging.DefaultInstance.SendAsync(message2);

        return res;
    }

    public async void Test()
    {
        var a = await SetUserToken(
            new SetUserTokenRequest(
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

    public async Task<IResult> DeleteUserToken(DeleteUserTokenRequest request)
    {
        try
        {

            var tokenResult = await FindUserToken(request.UserId);
            context.UserTokens.Remove(tokenResult);

            await context.SaveChangesAsync();
            return Results.Ok("Token deleted");
        }
        catch (Exception e)
        {
            return Results.Problem(statusCode: 500, title: "Token registration error");
        }
    }

    private async Task<UserToken?> FindUserToken(string userId)
    {
        return await context.UserTokens.Where(x => x.UserId == userId).FirstOrDefaultAsync();
    }
}
