namespace Balance_Support.Scripts.Main;

public static class ConstStorage
{
    public static readonly DateTime MoscowUtcNow = TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Russian Standard Time"));

    
    public const string ThirdPartyConfigs ="Third-party configs"; 
    
    public const string Firebase = "Firebase";
    
    public const string FirebaseCloudMessagingJson = "FirebaseCloudMessaging.json";
    
    public const string FirebaseDatabaseClientConfigJson = "FirebaseDatabaseClientConfig.json";

    public const string FirebaseAuthApiKey = "FirebaseAuthApiKey.json";

    public static string BasePath => AppDomain.CurrentDomain.BaseDirectory;

    public static string FirebaseConfigsPath => Path.Combine(BasePath,ThirdPartyConfigs, Firebase);
}