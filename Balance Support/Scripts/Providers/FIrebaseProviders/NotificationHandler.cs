
// using FireSharp;
// using FireSharp.Interfaces;
// using FireSharp.Response;
// using FireSharp.Config;
using Firebase.Database;
using Firebase.Database.Query;
// using Google.Apis.Auth.OAuth2;
using Newtonsoft.Json;
using Balance_Support.Interfaces;
using Balance_Support.Scripts.Extensions;
using Balance_Support.SerializationClasses;
using Balance_Support.DataClasses.Records.DeviceData;
using Balance_Support.DataClasses.Records.NotificationData;

namespace Balance_Support;

public class NotificationHandler
{
    private readonly IDatabaseDeviceProvider provider;

    public NotificationHandler(IDatabaseDeviceProvider provider)
    {
        this.provider = provider;
    }
    
    public async Task<IResult> GetNotification(NotificationRequest request)
    {
        var bank = await provider.GetBankBySimCardId(request.SimId);
        if (string.IsNullOrEmpty(bank))
            return Results.Problem(statusCode: 500, title: "Cannot find sim");
        
        
        return Results.Ok();
    }
}