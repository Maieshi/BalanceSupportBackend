// using Firebase.Auth;
using Firebase.Auth;
using FireSharp;
using FireSharp.Interfaces;
using FireSharp.Response;
using FireSharp.Config;
// using Google.Apis.Auth.OAuth2;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using FirebaseConfig = FireSharp.Config.FirebaseConfig;
using System.Linq;
using Balance_Support.Interfaces;
using Balance_Support.Scripts.Extensions;
using Balance_Support.SerializationClasses;
namespace Balance_Support;

public class DatabaseRecordsProvider
{
    private IFirebaseClient client;
    private  IDatabaseUserProvider userProvider;

    private Dictionary<string, List<string>> userDevicesRecordIds;

    private Dictionary<string, DeviceInfo> deviceInfos;
    
    public DatabaseRecordsProvider(IFirebaseClient client, IDatabaseUserProvider userProvider)
    {
        this.client = client;
        this.userProvider = userProvider;
        userDevicesRecordIds = new Dictionary<string, List<string>>();
        deviceInfos = new Dictionary<string, DeviceInfo>();
    }

    public async Task<IResult> RegisterDevice(DeviceRequestData deviceRequestData)
    {
        if(!userProvider.TryGetUserByRecordId(deviceRequestData.UserRecordId,out var user))return Results.Problem(statusCode: 500,    title: "Cannot find user by RecordId");
        
        try
        {
            var pushResponse = await client.PushAsync("Devices/", deviceRequestData.DeviceInfo);
            if(userDevicesRecordIds.ContainsKey(deviceRequestData.UserRecordId))
                userDevicesRecordIds[deviceRequestData.UserRecordId].Add(pushResponse.Result.name);
            else
                userDevicesRecordIds.Add(deviceRequestData.UserRecordId, new List<string>(){pushResponse.Result.name});
            deviceInfos.Add(pushResponse.Result.name, deviceRequestData.DeviceInfo);
            return Results.Created($"Devices/{pushResponse.Result.name}", deviceInfos);
        }
        catch (Exception ex)
        {
            return Results.Problem(detail: ex.Message, statusCode: 500, title: "An error occurred while register device in database");
        }
    }
    
    public async Task<IResult> UpdateDeviceData
}