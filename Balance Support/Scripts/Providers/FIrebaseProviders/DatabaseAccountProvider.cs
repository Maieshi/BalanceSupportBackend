// using Firebase.Auth;

using System.Collections.ObjectModel;
using System.Diagnostics;
using Firebase.Auth;
// using FireSharp;
// using FireSharp.Interfaces;
// using FireSharp.Response;
// using FireSharp.Config;
using Firebase.Database;
using Firebase.Database.Query;
// using Google.Apis.Auth.OAuth2;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using FirebaseConfig = FireSharp.Config.FirebaseConfig;
using System.Linq;
using Balance_Support.Interfaces;
using Balance_Support.Scripts.Extensions;
using Balance_Support.SerializationClasses;
using Balance_Support.DataClasses.Records.AccountData;
using Balance_Support.Scripts.Extensions.RecordExtenstions;

namespace Balance_Support;

public class DatabaseAccountProvider : IDatabaseAccountProvider
{
    private FirebaseClient client;
    private IDatabaseUserProvider userProvider;

    public DatabaseAccountProvider(FirebaseClient client, IDatabaseUserProvider userProvider)
    {
        this.client = client;
        this.userProvider = userProvider;
    }

    public async Task<IResult> RegisterAccount(DeviceRegisterRequest deviceRegisterRequest)
    {
        if (!await userProvider.IsUserWithIdExist(deviceRegisterRequest.UserId))
            return Results.Problem(statusCode: 500, title: "User not found");
        

        if (await IsAlreadyExistAccountWithGropAndDeviceId(deviceRegisterRequest.UserId,
                deviceRegisterRequest.AccountData.AccountGroup, deviceRegisterRequest.AccountData.DeviceId, deviceRegisterRequest.AccountData.SimSlot))
            return Results.Problem(statusCode: 500,
                title: "One account with same group and device id already registered");

        FirebaseObject<AccountData> userAccount = null;
        FirebaseObject<UserAccountRelationData> relaion = null;
        try
        {
            userAccount = await client.Child("Accounts").PostAsync(deviceRegisterRequest.AccountData.NewAccountData());

            relaion = await RegisterRelations(deviceRegisterRequest.UserId, userAccount);

            return Results.Created($"Accountes", JsonConvert.SerializeObject(userAccount));
        }
        catch (Exception ex)
        {
            if (userAccount != null)
            {
                await client
                    .Child("Devices")
                    .Child(userAccount.Key)
                    .DeleteAsync();
            }
            

            if (relaion != null)
            {
                await client
                    .Child("Relations")
                    .Child("User-Account")
                    .DeleteAsync();
            }

            return Results.Problem(detail: ex.Message, statusCode: 500,
                title: "An error occurred while registering device");
        }
    }

    public async Task<IResult> UpdateAccount(DeviceUpdateRequest deviceUpdateRequest)
    {
        try
        {
            var relation = await FindRelationByAccountId(deviceUpdateRequest.AccountId);
            
            
            var account = await FindAccountByAccountId(deviceUpdateRequest.AccountId);
            if (account == null)
                return Results.Problem(statusCode: 500, title: "Device not found");

            if (account.Object.DeviceId != deviceUpdateRequest.AccountDataRequest.DeviceId)
                return Results.BadRequest("Cannot change device id");
            
            if (await IsAlreadyExistAccountWithGropAndDeviceId(deviceUpdateRequest.UserId,
                    deviceUpdateRequest.AccountDataRequest.AccountGroup, deviceUpdateRequest.AccountDataRequest.DeviceId, deviceUpdateRequest.AccountDataRequest.SimSlot))
                return Results.Problem(statusCode: 500,
                    title: "One account with same group and device id already registered");

            await client
                .Child("Devices")
                .Child(account.Key)
                .PutAsync(deviceUpdateRequest.AccountDataRequest.NewAccountData());

            return Results.Ok($"Devices/{deviceUpdateRequest.AccountId}");
        }
        catch (Exception ex)
        {
            return Results.Problem(detail: ex.Message, statusCode: 500,
                title: "An error occurred while updating device ");
        }
    }
    
    

    public async Task<IResult> DeleteDevice(DeviceDeleteRequest deviceDeleteRequest)
    {
        try
        {
            var currentAccount = await FindAccountByAccountId(deviceDeleteRequest.AccountId);
            if (currentAccount == null)
                return Results.Problem(statusCode: 500, title: "Account not found");

            var relations = await FindRelationByAccountId(deviceDeleteRequest.AccountId);
            if (!relations.Any())
                return Results.Problem(statusCode: 500, title: "Relation not found");

            await client
                .Child("Accounts")
                .Child(currentAccount.Key)
                .DeleteAsync();

            await client
                    .Child("Relations")
                    .Child("User-Account")
                    .Child(currentAccount.Key)
                    .DeleteAsync();
            

            return Results.Ok($"Devices/{deviceDeleteRequest.AccountId}");
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return Results.Problem(statusCode: 500, title: "Cannot delete account");
        }
    }

    public async Task<IResult> GetAccountsByGroupAndDeviceId(DeviceGetRequest deviceGetRequest)
    {
        if (!await userProvider.IsUserWithIdExist(deviceGetRequest.UserId))
            return Results.Problem(statusCode: 500, title: "User not found");

        var relations = (await FindRelationByUserId(deviceGetRequest.UserId));

        if (!relations.Any())
            return Results.Problem(statusCode: 500, title: "Relations not found");

        var accounts = (await FindAccountsByUserId(deviceGetRequest.UserId))
            .Where(x =>
            x.Object.AccountGroup == deviceGetRequest.AccountGroup
            && x.Object.DeviceId == deviceGetRequest.DeviceId).ToList();

        if (accounts.Any())
            return Results.Problem(statusCode: 500, title: "Accounts not found");
       

        return Results.Ok(new
        {
            Accounts = accounts.Select(x => x.Object)
        });
    }

    public async void Test()
    {
        // var simcards = new List<SimCardData>()
        // {
        //     new SimCardData(
        //         "123123",
        //         "88005553535",
        //         "Sberbank",
        //         1488,
        //         500
        //     ),
        //     new SimCardData(
        //         "456456",
        //         "81234567890",
        //         "Tinkoff",
        //         0228,
        //         1000
        //     )
        // };
        // var registerUser = await userProvider.CreateNewUserAsync(new UserAuthData(){Id = "safsdgsdfg", Email = "testuser6@gmail", DisplayName = "testuser6"});
        // var checkSims = await FindSimcardMany(simcards);
        // var checkSim = await FindSimcard(simcards[0].SimCardId);
        // var checkDevice = await FindAccountByAccountId("sDAmWae7RqMsmWIC74lVdLuQRpq1");
        // var checkSimmmm = await FindSimcarddd(simcards[0].SimCardId);
        // var simDef = checkSimmmm.FirstOrDefault();
        // var checkDef = simDef == default;
        // var checkNull = simDef == null;
        // var checkSimm2 = await FindSimcarddd2(simcards[0].SimCardId);
        // var checkDef2 = checkSimm2 == default;
        // var checkNull2 = checkSimm2 == null;
        // var registerDevice = await RegisterDevice(
        //     new DeviceRegisterRequest(
        //         "sDAmWae7RqMsmWIC74lVdLuQRpq1",
        //         new DeviceData(
        //             "asefasdf",
        //             "Ivanov",
        //             3,
        //             1,
        //             "Very rich person"
        //         ),
        //         simcards
        //         
        //     )
        // );

        // var updatateDevice = await UpdateDeviceData(
        //     new DeviceUpdateRequest("asefasdf", new DeviceData("asefasdf", "petrov", 2, 2, "Very poor person"))
        // );

    //     var deleteDevice = await DeleteDevice(new DeviceDeleteRequest("asefasdf"));
    //
    //     Debug.Print(deleteDevice.ToString());
    }

    private async Task<FirebaseObject<UserAccountRelationData>> RegisterRelations(string userId, FirebaseObject<AccountData> account)
        => await client
            .Child("Relations")
            .Child("User-Account")
            .PostAsync(new UserAccountRelationData(
                userId,
                account.Object.AccountId,
                account.Key));


    // private async Task<bool> IsSimcardsExists(List<SimCardData> simcards)
    //     => (await FindSimcardMany(simcards)).Any();
    //
    //
    // private async Task<bool> IsSimcardExists(string simcardId)
    //     => await FindSimcard(simcardId) != null;
    //
    // private async Task<FirebaseObject<SimCardData>?> FindSimcard(string simcardId)
    //     => (await client
    //         .Child("Simcards")
    //         .OrderBy("SimId")
    //         .EqualTo(simcardId)
    //         .OnceAsync<SimCardData>()).FirstOrDefault();
    //
    // private async Task<List<FirebaseObject<SimCardData>>> FindSimcardMany(List<SimCardData> simcards)
    //     => (
    //             await Task.WhenAll(
    //                 simcards.Select(
    //                     async sim => await FindSimcard(sim.SimCardId)
    //                 )
    //             )
    //         )
    //         .Where(s => s != null)
    //         .Cast<FirebaseObject<SimCardData>>()
    //         .ToList();
    //
    // private async Task<List<FirebaseObject<SimCardData>>> FindSimcardMany(List<string> simcards)
    //     => (
    //             await Task.WhenAll(
    //                 simcards.Select(
    //                     async sim => await FindSimcard(sim)
    //                 )
    //             )
    //         )
    //         .Where(s => s != null)
    //         .Cast<FirebaseObject<SimCardData>>()
    //         .ToList();

    // public async Task<List<SimCardData>> GetSimCardsByDeviceId(string deviceId)
    //     => (await FindSimcardMany(
    //                 (await FindRelationByAccountId(deviceId))
    //                 .Select(x => x.Object.SimCardId)
    //                 .ToList()
    //             )
    //         )
    //         .Select(x => x.Object)
    //         .ToList();

    private async Task<bool> IsAccountExists(string accountId)
        => await FindAccount(accountId) != null;

    private async Task<AccountData?> FindAccount(string accountData)
    {
        var deviceInfo = await FindAccountByAccountId(accountData);
        if (deviceInfo != default)
            return deviceInfo.Object;

        deviceInfo = await FindAccountByRecordId(accountData);
        return deviceInfo?.Object;
    }


    private async Task<FirebaseObject<AccountData>?> FindAccountByRecordId(string recordId)
        => (await client
            .Child("Accounts")
            .Child(recordId)
            .OnceAsync<AccountData>()).FirstOrDefault();


    public async Task<FirebaseObject<AccountData>?> FindAccountByAccountId(string accountId)
        => (await client
            .Child("Accounts")
            .OrderBy("AccountId")
            .EqualTo(accountId)
            .OnceAsync<AccountData>()).FirstOrDefault();
     

    private async Task<IReadOnlyCollection<FirebaseObject<UserAccountRelationData>>>
        FindRelationByAccountId(string deviceId)
        => await client
            .Child("Relations")
            .Child("User-Account")
            .OrderBy("AccountId")
            .EqualTo(deviceId)
            .OnceAsync<UserAccountRelationData>();


    private async Task<IReadOnlyCollection<FirebaseObject<UserAccountRelationData>>>
        FindRelationByUserId(string userId)
        => await client
            .Child("Relations")
            .Child("User-Account")
            .OrderBy("UserId")
            .EqualTo(userId)
            .OnceAsync<UserAccountRelationData>();


    private async Task<ReadOnlyCollection<FirebaseObject<AccountData>>> FindAccountsByUserId(string userId)
        => (await Task.WhenAll(
                    (await FindRelationByUserId(userId))
                    .Select(r => r.Object.AccountRecordId)
                    .Where(id => !string.IsNullOrEmpty(id))
                    .Select(FindAccountByRecordId)
                )
            )
            .Where(device => device != null)
            .Cast<FirebaseObject<AccountData>>() // Explicitly cast to non-nullable type
            .ToList()
            .AsReadOnly();

    private async Task<bool> IsAlreadyExistAccountWithGropAndDeviceId(string userId, int accountGroup,
        int deviceId, int simSlot )
    {
        return (await FindAccountsByUserId(userId)).Any(device =>
            device.Object.AccountGroup == accountGroup && device.Object.DeviceId == deviceId&& device.Object.SimSlot == simSlot);
    }
}