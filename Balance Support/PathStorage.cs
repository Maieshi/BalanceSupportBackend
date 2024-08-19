namespace Balance_Support;

public static class PathStorage
{
    public const string ThirdPartyConfigs ="Third-party configs"; 
    
    public const string Firebase = "Firebase";
    
    public const string FirebaseCloudMessagingJson = "balance-support-b9da3-firebase-adminsdk-ingxx-2394611a79.json";
    
    public const string FirebaseDatabaseClientConfigJson = "FirebaseDatabaseClientConfig.json";

    public const string FirebaseAuthApiKey = "FirebaseAuthApiKey.json";

    public static string BasePath => AppDomain.CurrentDomain.BaseDirectory;

    public static string FirebaseConfigsPath => Path.Combine(BasePath,ThirdPartyConfigs, Firebase);
}