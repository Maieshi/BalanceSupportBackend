using Balance_Support.Interfaces;
using Balance_Support.SerializationClasses;
using Newtonsoft.Json;
using Firebase.Auth;
using FireSharp;
using FireSharp.Interfaces;
using FirebaseConfig = FireSharp.Config.FirebaseConfig;
using FirebaseAdmin;
using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;

namespace Balance_Support;

public static class ServicesInitializer
{
    public static void Initialize(IServiceCollection services)
    {
        FirebaseAuthApiKey apiKey = JsonConvert.DeserializeObject<FirebaseAuthApiKey>(
            File.ReadAllText(Path.Combine(PathStorage.FirebaseConfigsPath, PathStorage.FirebaseAuthApiKey)));

        
        
        FirebaseDatabaseClientConfig databseConfig = JsonConvert.DeserializeObject<FirebaseDatabaseClientConfig>(
            File.ReadAllText(
                Path.Combine(PathStorage.FirebaseConfigsPath, PathStorage.FirebaseDatabaseClientConfigJson)));

        FirebaseApp.Create(new AppOptions()
        {
            Credential = GoogleCredential.FromFile(Path.Combine(PathStorage.FirebaseConfigsPath, PathStorage.FirebaseCloudMessagingJson)),
        });
        
        services.AddSingleton<IFirebaseClient>(
            new FirebaseClient(
                new FirebaseConfig()
                {
                    AuthSecret = databseConfig.AuthSecret,
                    BasePath = databseConfig.BasePath
                }
            )
        );
        ;
        services.AddSingleton<IFirebaseAuthProvider>(
            new FirebaseAuthProvider(
                new Firebase.Auth.FirebaseConfig(apiKey.ApiKey)
                )
            );

        services.AddSingleton<IDatabaseUserProvider, DatabaseUserProvider>();

        services.AddSingleton<IAuthUserProvider, AuthUserProvider>();
    }
}