// using Firebase.Auth;

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

    // private Dictionary<string, List<string>> userDevicesRecordIds;
    //
    // private Dictionary<string, DeviceInfo> deviceInfos;

    public DatabaseDeviceProvider(FirebaseClient client, IDatabaseUserProvider userProvider)
    {
        this.client = client;
        this.userProvider = userProvider;
        // userDevicesRecordIds = new Dictionary<string, List<string>>();
        // deviceInfos = new Dictionary<string, DeviceInfo>();
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
        IEnumerable<FirebaseObject<UserSimcardDeviceRelationData>> relaions = Enumerable.Empty<FirebaseObject<UserSimcardDeviceRelationData>>()
        try
        {
            postSimcardsList = await RegisterSimcards(deviceRegisterRequest.SimcardsData);
            
            userDevice = await client.Child("Devices").PostAsync(deviceRegisterRequest.DeviceData);
            
            relaions = await RegisterRelations(
                deviceRegisterRequest.UserId,
                deviceRegisterRequest.DeviceData.DeviceId, 
                deviceRegisterRequest.SimcardsData.Select(s => s.SimCardId).ToList());
            
            return Results.Created($"Relations/User-Device-Simcard",JsonConvert.SerializeObject(relaions));
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
                title: "An error occurred while registering device in the database");
        }
    }

    public async Task<IResult> UpdateDeviceData(DeviceUpdateRequest deviceRequestRequest)
    {
        if (!await IsDeviceExists(deviceRequestRequest.DeviceData.DeviceId))
            return Results.Problem(statusCode: 500, title: "Device not found");
        
        try
        {
            var setResponse = await client.SetAsync($"Devices/{deviceRequestRequest.DeviceRecordId}",
                deviceRequestRequest.DeviceData);
            deviceInfos[deviceRequestRequest.DeviceRecordId] = deviceRequestRequest.DeviceData;
            return Results.Ok($"Devices/{deviceRequestRequest.DeviceRecordId}");
        }
        catch (Exception ex)
        {
            return Results.Problem(detail: ex.Message, statusCode: 500,
                title: "An error occurred while register device in database");
        }
    }

    public async Task<IResult> DeleteDeviceData(DeviceDeleteRequest deviceDeleteRequest)
    {
        if (!userProvider.TryGetUserByRecordId(deviceDeleteRequest.UserRecordId, out var user))
            return Results.Problem(statusCode: 500, title: "Cannot find user");
        if (!deviceInfos.TryGetValue(deviceDeleteRequest.DeviceId, out var deviceInfo))
            return Results.Problem(statusCode: 500, title: "Cannot find user device");
        try
        {
            var setResponse = await client.DeleteAsync($"Devices/{deviceInfo}");
            userDevicesRecordIds[deviceDeleteRequest.UserRecordId].Remove(deviceDeleteRequest.DeviceId);
            deviceInfos.Remove(deviceDeleteRequest.DeviceId);
            return Results.Ok($"Devices/{deviceDeleteRequest.DeviceId}");
        }
        catch (Exception ex)
        {
            return Results.Problem(detail: ex.Message, statusCode: 500,
                title: "An error occurred while register device in database");
        }
    }

    public async void Test()
    {
        // var registerUser = await userProvider.CreateNewUserAsync(new UserAuthData(){Id = "safsdgsdfg", Email = "testuser6@gmail", DisplayName = "testuser6"});

        var registerDevice = await RegisterDevice(
            new DeviceRegisterRequest(
                "sDAmWae7RqMsmWIC74lVdLuQRpq1",
                new DeviceData(
                    "1",
                    "Ivanov",
                    3,
                    1,
                    new List<SimCardData>()
                    {
                        new SimCardData(
                            1,
                            "88005553535",
                            "Sberbank",
                            1488,
                            500
                        ),
                        new SimCardData(
                            2,
                            "81234567890",
                            "Tinkoff",
                            0228,
                            1000
                        )
                    },
                    "Very rich person"
                )
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
    
    private async Task<IEnumerable<FirebaseObject<UserSimcardDeviceRelationData>>> RegisterRelations(string UserId,string DeviceId,List<string> simcardIds)
        => await Task.WhenAll(
            simcardIds.Select(
                simId => client
                    .Child("Relations")
                    .Child("User-Device-Simcard")
                    .PostAsync(new UserSimcardDeviceRelationData(UserId,DeviceId,simId))
            )
        );

    private async Task<List<SimCardData>> FindSimcardsByIds(List<SimCardData> simcardIds)
        => (
                await Task.WhenAll(
                    simcardIds.Select(
                        id => client
                            .Child("Simcards")
                            .OrderBy("SimId")
                            .EqualTo(id.SimCardId)
                            .OnceSingleAsync<SimCardData>()
                    )
                )
            )
            .Where(s => s != null)
            .ToList();


    private async Task<bool> IsSimcardsExists(List<SimCardData> simcards)
        => await FindSimcardsByIds(simcards) != null;


    private async Task<bool> IsSimcardExists(string simcardId)
        => await FindSimcard(simcardId) != null;

    private async Task<SimCardData> FindSimcard(string simcardId)
        => await client
            .Child("Simcards")
            .OrderBy("SimId")
            .EqualTo(simcardId)
            .OnceSingleAsync<SimCardData>();


    private async Task<bool> IsDeviceExists(string userId)
        => await FindDevice(userId) != null;

    private async Task<DeviceData> FindDevice(string deviceData)
    {
        var deviceInfo = await FindDeviceByDeviceId(deviceData);
        if (deviceInfo != null)
            return deviceInfo;

        deviceInfo = await FindDeviceByRecordId(deviceData);
        if (deviceInfo != null)
            return deviceInfo;


        return null;
    }


    private async Task<DeviceData> FindDeviceByRecordId(string deviceRecordId)
        => await client
            .Child("Devices")
            .Child(deviceRecordId)
            .OnceSingleAsync<DeviceData>();


    private async Task<DeviceData> FindDeviceByDeviceId(string deviceId)
        => await client
            .Child("Devices")
            .OrderBy("DeviceId")
            .EqualTo(deviceId)
            .OnceSingleAsync<DeviceData>();
}