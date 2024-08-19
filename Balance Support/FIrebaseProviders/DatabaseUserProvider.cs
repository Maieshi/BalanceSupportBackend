using System.Diagnostics;
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
using Balance_Support.SerializationClasses;
namespace Balance_Support;

public class DatabaseUserProvider
{
    private const string DatabaseUrl = "https://balance-support-b9da3-default-rtdb.europe-west1.firebasedatabase.app/";

    private const string DatabaseSecret = "3J23Se6pRRrvuTiyPKSuLRbIB94GM4jtTqmuf6fe";

    // private FirebaseClient client;

    private IFirebaseClient client;

    private IFirebaseConfig config;


    private Dictionary<string, UserAuthData> usersCache;

    public DatabaseUserProvider()
    {
        config = new FireSharp.Config.FirebaseConfig()
        {
            AuthSecret = DatabaseSecret,
            BasePath = DatabaseUrl
        };
        client = new FireSharp.FirebaseClient(config);
        usersCache = new Dictionary<string, UserAuthData>();
    }

    public DatabaseUserProvider(IFirebaseClient client)
    {
        this.client = client;
        usersCache = new Dictionary<string, UserAuthData>();
    }

    public async Task<string> CreateNewUserAsync(UserAuthData newUser)
    {
        try
        {
            var pushResponse = await client.PushAsync("Users/", newUser);
            usersCache.Add(pushResponse.Result.name, newUser);
            return pushResponse.Result.name;
        }
        catch (Exception e)
        {
            return String.Empty;
        }
    }


    // public async Task<bool> IsUserExistsAsync(string userCred)
    // {
    //     
    //     try
    //     { 
    //         var response = await client.GetAsync("Users");
    //
    //         if (string.IsNullOrEmpty(response.Body)) return false;
    //
    //         List<UserAuthData> users = ParseUsersToList(response.Body);
    //         return users.Exists(x =>
    //             string.Equals(x.Email, userCred, StringComparison.OrdinalIgnoreCase) ||
    //             string.Equals(x.DisplayName, userCred, StringComparison.OrdinalIgnoreCase));
    //     }
    //     catch (Exception e)
    //     {
    //         Console.WriteLine(e);
    //         return false;
    //     }
    // }
    //
    // public bool IsUserExists(string userCred)
    // {
    //     try
    //     {
    //         var response = client.Get("Users");
    //
    //         Debug.Print(response.Body + string.IsNullOrEmpty(response.Body));
    //
    //
    //         if (string.IsNullOrEmpty(response.Body) || response.Body == "null") return false;
    //
    //         // Use LINQ to convert the dictionary to a list of UserAuthData
    //         List<UserAuthData> users = ParseUsersToList(response.Body);
    //         return users.Exists(x =>
    //             string.Equals(x.Email, userCred, StringComparison.OrdinalIgnoreCase) ||
    //             string.Equals(x.DisplayName, userCred, StringComparison.OrdinalIgnoreCase));
    //     }
    //     catch (Exception e)
    //     {
    //         Console.WriteLine(e);
    //         return false;
    //     }
    // }
    
    public bool TryGetUser(string userCred, out UserAuthData user)
    {
        if(TryGetCachedUserByCred(userCred, out user)) return true;
        
        UpdateUsersCache();
        
        return (user = usersCache.Values.FirstOrDefault(x =>
            string.Equals(x.Email, userCred, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(x.DisplayName, userCred, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(x.Id, userCred, StringComparison.OrdinalIgnoreCase)))!=null;
    }

    public bool TryGetUserByRecordId(string recordId, out UserAuthData user)
    {
        
        if(TryGetCachedUserByRecordId(recordId, out user)) return true;
        
        UpdateUsersCache();

        return usersCache.TryGetValue(recordId, out user);
    }
    
    #region Private 

    private List<UserAuthData> ParseUsersToList(string response)
        => JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, string>>>(response)
            .Values
            .Select(user => new UserAuthData
            {
                Id = user["Id"],
                Email = user["Email"],
                DisplayName = user["DisplayName"]
            })
            .ToList();
    
    
    private Dictionary<string, UserAuthData> ParseUsersToDict(string response)
        => JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, string>>>(response)
            .ToDictionary(
                kvp => kvp.Key,
                kvp => new UserAuthData
                {
                    Id = kvp.Value["Id"],
                    Email = kvp.Value["Email"],
                    DisplayName = kvp.Value["DisplayName"]
                }
            );
    
    
    private void UpdateUsersCache()
    {
        try
        {
            var response = client.Get("Users");
            var parsedUsers = ParseUsersToDict(response.Body);
            foreach (var parsedUser in parsedUsers)
            {
                if(usersCache.ContainsKey(parsedUser.Key)) usersCache[parsedUser.Key] = parsedUser.Value;
                else usersCache.Add(parsedUser.Key, parsedUser.Value);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

   

    private bool TryGetCachedUserByRecordId(string recordId, out UserAuthData user)
        => usersCache.TryGetValue(recordId, out user);

    private bool TryGetCachedUserByCred(string userCred, out UserAuthData user)
        => (user = (from cachedUser in usersCache.Values
            where string.Equals(cachedUser.DisplayName, userCred, StringComparison.OrdinalIgnoreCase)
            ||  string.Equals(cachedUser.Email, userCred, StringComparison.OrdinalIgnoreCase)
            select cachedUser).FirstOrDefault()) != null;
    

    #endregion
}

