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
using Balance_Support.DataClasses.Records.DeviceData;

namespace Balance_Support;

public class DatabaseDeviceProvider : IDatabaseDeviceProvider
{
    private FirebaseClient client;
    private IDatabaseUserProvider userProvider;

    public DatabaseDeviceProvider(FirebaseClient client, IDatabaseUserProvider userProvider)
    {
        this.client = client;
        this.userProvider = userProvider;
    }

    public async Task<IResult> RegisterDevice(DeviceRegisterRequest deviceRegisterRequest)
    {
        if (!await userProvider.IsUserWithIdExist(deviceRegisterRequest.UserId))
            return Results.Problem(statusCode: 500, title: "User not found");

        if (await IsDeviceExists(deviceRegisterRequest.DeviceData.DeviceId))
            return Results.Problem(statusCode: 500, title: "Device already registered");

        if (await IsSimcardsExists(deviceRegisterRequest.SimcardsData))
            return Results.Problem(statusCode: 500, title: "One of sim cars already registered");
        FirebaseObject<DeviceData> userDevice = null;
        IEnumerable<FirebaseObject<SimCardData>> postSimcardsList = Enumerable.Empty<FirebaseObject<SimCardData>>();
        IEnumerable<FirebaseObject<UserDeviceSimСardRelationData>> relaions =
            Enumerable.Empty<FirebaseObject<UserDeviceSimСardRelationData>>();
        try
        {
            postSimcardsList = await RegisterSimcards(deviceRegisterRequest.SimcardsData);

            userDevice = await client.Child("Devices").PostAsync(deviceRegisterRequest.DeviceData);

            relaions = await RegisterRelations(
                deviceRegisterRequest.UserId,
                userDevice,
                postSimcardsList);

            return Results.Created($"Relations/User-Device-Simcard", JsonConvert.SerializeObject(relaions));
        }
        catch (Exception ex)
        {
            if (userDevice != null)
            {
                await client
                    .Child("Devices")
                    .Child(userDevice.Key)
                    .DeleteAsync();
            }

            if (postSimcardsList.Any())
            {
                foreach (var sim in postSimcardsList)
                {
                    await client
                        .Child("Simcards")
                        .Child(sim.Key)
                        .DeleteAsync();
                }
            }

            if (relaions.Any())
            {
                foreach (var sim in relaions)
                {
                    await client
                        .Child("Relations")
                        .Child("User-Device-Simcard")
                        .DeleteAsync();
                }
            }

            return Results.Problem(detail: ex.Message, statusCode: 500,
                title: "An error occurred while registering device");
        }
    }

    public async Task<IResult> UpdateDeviceData(DeviceUpdateRequest deviceRequestRequest)
    {
        try
        {
            var currentDevice = await FindDeviceByDeviceId(deviceRequestRequest.DeviceData.DeviceId);
            if (currentDevice == null)
                return Results.Problem(statusCode: 500, title: "Device not found");

            if (currentDevice.Object.DeviceId != deviceRequestRequest.DeviceData.DeviceId)
                return Results.BadRequest("Cannot change device id");

            await client
                .Child("Devices")
                .Child(currentDevice.Key)
                .PutAsync(deviceRequestRequest.DeviceData);

            return Results.Ok($"Devices/{deviceRequestRequest.DeviceId}");
        }
        catch (Exception ex)
        {
            return Results.Problem(detail: ex.Message, statusCode: 500,
                title: "An error occurred while updating device ");
        }
    }

    public async Task<IResult> DeleteDeviceData(DeviceDeleteRequest deviceDeleteRequest)
    {
        try
        {
            var currentDevice = await FindDeviceByDeviceId(deviceDeleteRequest.DeviceId);
            if (currentDevice == null)
                return Results.Problem(statusCode: 500, title: "Device not found");

            var relations = await FindRelationByDeviceId(deviceDeleteRequest.DeviceId);
            if (!relations.Any())
                return Results.Problem(statusCode: 500, title: "Relation not found");

            await client
                .Child("Devices")
                .Child(currentDevice.Key)
                .DeleteAsync();
            
            foreach (var relation in relations)
            {
                await client
                    .Child("SimCards")
                    .Child(relation.Object.SimCardRecordId)
                    .DeleteAsync();
            }
            
            return Results.Ok($"Devices/{deviceDeleteRequest.DeviceId}");
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return Results.Problem(statusCode: 500, title: "Cannot delete device");
        }
    }

    public async void Test()
    {
        var simcards = new List<SimCardData>()
        {
            new SimCardData(
                "123123",
                "88005553535",
                "Sberbank",
                1488,
                500
            ),
            new SimCardData(
                "456456",
                "81234567890",
                "Tinkoff",
                0228,
                1000
            )
        };
        // var registerUser = await userProvider.CreateNewUserAsync(new UserAuthData(){Id = "safsdgsdfg", Email = "testuser6@gmail", DisplayName = "testuser6"});
        var checkSims = await FindSimcardMany(simcards);
        var checkSim = await FindSimcard(simcards[0].SimCardId);
        var checkDevice = await FindDeviceByDeviceId("sDAmWae7RqMsmWIC74lVdLuQRpq1");
        var checkSimmmm = await FindSimcarddd(simcards[0].SimCardId);
        var simDef = checkSimmmm.FirstOrDefault();
        var checkDef = simDef == default;
        var checkNull = simDef == null;
        var checkSimm2 = await FindSimcarddd2(simcards[0].SimCardId);
        var checkDef2 = checkSimm2 == default;
        var checkNull2 = checkSimm2 == null;
        var registerDevice = await RegisterDevice(
            new DeviceRegisterRequest(
                "sDAmWae7RqMsmWIC74lVdLuQRpq1",
                new DeviceData(
                    "asefasdf",
                    "Ivanov",
                    3,
                    1,
                    "Very rich person"
                ),
                simcards
                
            )
        );

        Debug.Print(registerDevice.ToString());
    }

    private async Task<IEnumerable<FirebaseObject<SimCardData>>> RegisterSimcards(List<SimCardData> simcards)
        => await Task.WhenAll(
            simcards.Select(
                simcard => client
                    .Child("SimCards")
                    .PostAsync(simcard)
            )
        );

    private async Task<IEnumerable<FirebaseObject<UserDeviceSimСardRelationData>>> RegisterRelations(string userId,
        FirebaseObject<DeviceData> device, IEnumerable<FirebaseObject<SimCardData>> simcards)
        => await Task.WhenAll(
            simcards.Select(
                simcard => client
                    .Child("Relations")
                    .Child("User-Device-Simcard")
                    .PostAsync(new UserDeviceSimСardRelationData(
                        userId, 
                        device.Key, 
                        device.Object.DeviceId,
                        simcard.Key, 
                        simcard.Object.SimCardId))
            )
        );

    


    private async Task<bool> IsSimcardsExists(List<SimCardData> simcards)
        => (await FindSimcardMany(simcards)).Any() ;


    private async Task<bool> IsSimcardExists(string simcardId)
        => await FindSimcard(simcardId) != null;

    private async Task<FirebaseObject<SimCardData>?> FindSimcard(string simcardId)
        => (await client
            .Child("Simcards")
            .OrderBy("SimId")
            .EqualTo(simcardId)
            .OnceAsync<SimCardData>()).FirstOrDefault();
    
    private async Task<List<FirebaseObject<SimCardData>?>> FindSimcardMany(List<SimCardData> simcardIds)
        => (
                await Task.WhenAll(
                    simcardIds.Select(
                        async sim => await FindSimcard(sim.SimCardId)
                    )
                )
            )
            .Where(s => s != null)
            .ToList();

    private async Task<IReadOnlyCollection<FirebaseObject<SimCardData>>> FindSimcarddd(string simcardId)
        => await client
            .Child("Simcards")
            .OrderBy("SimId")
            .EqualTo(simcardId)
            .OnceAsync<SimCardData>();
    
    private async Task<FirebaseObject<SimCardData>?> FindSimcarddd2(string simcardId)
        => (await client
            .Child("Simcards")
            .OrderBy("SimId")
            .EqualTo(simcardId)
            .OnceAsync<SimCardData>()).FirstOrDefault();

    private async Task<bool> IsDeviceExists(string userId)
        => await FindDevice(userId) != null;

    private async Task<DeviceData?> FindDevice(string deviceData)
    {
        var deviceInfo = await FindDeviceByDeviceId(deviceData);
        if (deviceInfo != default)
            return deviceInfo.Object;

        deviceInfo = await FindDeviceByRecordId(deviceData);
        if (deviceInfo != null)
            return deviceInfo.Object;


        return null;
    }


    private async Task<FirebaseObject<DeviceData>?> FindDeviceByRecordId(string deviceRecordId)
        => (await client
            .Child("Devices")
            .Child(deviceRecordId)
            .OnceAsync<DeviceData>()).FirstOrDefault();


    private async Task<FirebaseObject<DeviceData>?> FindDeviceByDeviceId(string deviceId)
        => (await client
            .Child("Devices")
            .OrderBy("DeviceId")
            .EqualTo(deviceId)
            .OnceAsync<DeviceData>()).FirstOrDefault();

    // private async Task<IReadOnlyCollection<FirebaseObject<UserDeviceSimСardRelationData>>> FindRelation(
    //     string relationData)
    // {
    //     var relationByUserId = await client
    //         .Child("Relations")
    //         .Child("User-Device-Simcard")
    //         .OrderBy("UserId")
    //         .EqualTo(relationData)
    //         .OnceAsync<UserDeviceSimСardRelationData>();
    //
    //     if (!relationByUserId.Any())
    //         return relationByUserId;
    //
    //     var relationByDeviceId = await client
    //         .Child("Relations")
    //         .Child("User-Device-Simcard")
    //         .OrderBy("UserId")
    //         .EqualTo(relationData)
    //         .OnceAsync<UserDeviceSimСardRelationData>();
    //
    //     if (!relationByUserId.Any())
    //         return relationByDeviceId;
    //
    //     var relationBySimId = await client
    //         .Child("Relations")
    //         .Child("User-Device-Simcard")
    //         .OrderBy("SimCardId")
    //         .EqualTo(relationData)
    //         .OnceAsync<UserDeviceSimСardRelationData>();
    //
    //     if (!relationBySimId.Any())
    //         return relationByDeviceId;
    //
    //     return ReadOnlyCollection<FirebaseObject<UserDeviceSimСardRelationData>>.Empty;
    // }

    private async Task<IReadOnlyCollection<FirebaseObject<UserDeviceSimСardRelationData>>>
        FindRelationBySimId(string simId)
        => await client
            .Child("Relations")
            .Child("User-Device-Simcard")
            .OrderBy("SimCardId")
            .EqualTo(simId)
            .OnceAsync<UserDeviceSimСardRelationData>();

    private async Task<IReadOnlyCollection<FirebaseObject<UserDeviceSimСardRelationData>>>
        FindRelationByDeviceId(string simId)
        => await client
            .Child("Relations")
            .Child("User-Device-Simcard")
            .OrderBy("UserId")
            .EqualTo(simId)
            .OnceAsync<UserDeviceSimСardRelationData>();
}