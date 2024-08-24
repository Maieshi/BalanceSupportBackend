using Balance_Support.SerializationClasses;
using Firebase.Database;
using Firebase.Database.Query;
using Google.Apis.Auth.OAuth2;
namespace Balance_Support;

public class FbTest
{
FirebaseClient firebaseClient;
    public FbTest()
    {
         firebaseClient =
            new FirebaseClient("https://balance-support-b9da3-default-rtdb.europe-west1.firebasedatabase.app/",
                new FirebaseOptions { AuthTokenAsyncFactory = () => GetTokenByGoogleServices(), AsAccessToken = true });
    }
    
    public async void Test()
    {
        var users = await firebaseClient.Child("Users").OrderBy("DisplayName").EqualTo("testuser6").OnceAsync<UserAuthData>();
        
       var user =  await firebaseClient.Child("Users").PostAsync(new UserAuthData(){Id = "aaaaa", Email = "aaaaa@gmail.com", DisplayName = "aaaaaUser"});
        foreach (var dino in users)
        {
            Console.WriteLine($"{dino.Key} is {dino.Object.DisplayName}m high.");
        }
    }
    
    private async Task<string> GetTokenByGoogleServices()
    {     
        var credential = GoogleCredential.FromFile(Path.Combine(PathStorage.FirebaseConfigsPath,PathStorage.FirebaseCloudMessagingJson)).CreateScoped(new string[] {
            "https://www.googleapis.com/auth/userinfo.email",
            "https://www.googleapis.com/auth/firebase.database"
        });

        ITokenAccess c = credential as ITokenAccess;
        return await c.GetAccessTokenForRequestAsync();
    }
}