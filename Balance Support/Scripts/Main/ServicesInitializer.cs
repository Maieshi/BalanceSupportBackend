using System.Diagnostics;
using Balance_Support.DataClasses.Validators;
using Balance_Support.Interfaces;
using Balance_Support.SerializationClasses;
using Newtonsoft.Json;
using Firebase.Auth;
// using FireSharp;
// using FireSharp.Interfaces;
// using FirebaseConfig = FireSharp.Config.FirebaseConfig;
using FirebaseAdmin;
using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Firebase.Database;
using Firebase.Database.Query;
using Google.Apis.Auth.OAuth2;

using FluentValidation;
using FluentValidation.AspNetCore;
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
        
        services.AddFluentValidation(fv => fv.RegisterValidatorsFromAssemblyContaining<DeviceUpdateRequestValidator>());
        
        services.AddSingleton<FirebaseClient>(
            new FirebaseClient("https://balance-support-b9da3-default-rtdb.europe-west1.firebasedatabase.app/",
                new FirebaseOptions { AuthTokenAsyncFactory = () => GetTokenByGoogleServices(), AsAccessToken = true })
        );
        ;
        services.AddSingleton<IFirebaseAuthProvider>(
            new FirebaseAuthProvider(
                new Firebase.Auth.FirebaseConfig(apiKey.ApiKey)
                )
            );

        services.AddSingleton<IDatabaseUserProvider, DatabaseUserProvider>();

        services.AddSingleton<IAuthUserProvider, AuthUserProvider>();
        
        services.AddSingleton<IDatabaseAccountProvider, DatabaseAccountProvider>();

        services.AddSingleton<ICloudMessagingProvider, CloudMessagingProvider>();
        
        services.AddSingleton<IDatabaseTransactionProvider, DatabaseTransactionProvider>();

        services.AddSingleton<INotificationHandler, NotificationHandler>();

        // services.AddSingleton<FbTest>();
        
        var provider = services.BuildServiceProvider();
        // provider.GetService<IDatabaseDeviceProvider>().Test();
        // provider.GetService<CloudMessagingProvider>().Test();
        // provider.GetService<IDatabaseAccountProvider>().Test();
        // provider.GetService<ICloudMessagingProvider>().Test();
        provider.GetService<INotificationHandler>().Test();
        

    }
    private static async Task<string> GetTokenByGoogleServices()
    {     
        var credential = GoogleCredential.FromFile(Path.Combine(PathStorage.FirebaseConfigsPath,PathStorage.FirebaseCloudMessagingJson)).CreateScoped(new string[] {
            "https://www.googleapis.com/auth/userinfo.email",
            "https://www.googleapis.com/auth/firebase.database"
        });

        ITokenAccess c = credential as ITokenAccess;
        return await c.GetAccessTokenForRequestAsync();
    }
}