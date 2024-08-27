
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
using System.Text.RegularExpressions;

namespace Balance_Support;

public class NotificationHandler
{
    private readonly DatabaseDeviceProvider provider;

    public NotificationHandler(DatabaseDeviceProvider provider)
    {
        this.provider = provider;
    }
    
    public async Task<IResult> RegisterNotificationData(NotificationRequest request)
    {
        var match = Regex.Match(request.NotificationText, @"\b(?:MIR-|СЧЁТ|)(\d{4})\b");
        
       var simcards = await provider.GetSimCardsByDeviceId(request.DeviceId);
        
       
        
        return Results.Ok();
    }
}