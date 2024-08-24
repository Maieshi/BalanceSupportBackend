using System.Diagnostics;
// using Firebase.Auth;
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
using Balance_Support.SerializationClasses;

namespace Balance_Support;

public class DatabaseUserProvider : IDatabaseUserProvider
{
    private FirebaseClient client;

    public DatabaseUserProvider(FirebaseClient client)
    {
        this.client = client;
    }

    public async Task<string> CreateNewUserAsync(UserAuthData newUser)
    {
        try
        {
            var postResponse = await client.Child("Users").PostAsync(newUser);
            return postResponse.Key;
        }
        catch (Exception e)
        {
            return String.Empty;
        }
    }

    public async Task<UserAuthData> GetUser(string userCred)
    {
        var userByDisplayName = await GetUserByDisplayName(userCred);
        if (userByDisplayName != null)
        {
            return userByDisplayName;
        }
        
        var userByEmail = await GetUserByEmail(userCred);
        if (userByEmail != null)
        {
            return userByEmail;
        }
        
        var userById = await GetUserById(userCred);
        if (userById != null)
        {
            return userById;
        }
        
        var userByRecordId = await GetUserByRecordId(userCred);
        if (userByRecordId != null)
        {
            return userByRecordId;
        }
// Return null if no match is found
        return null;
    }
    
    public async Task<bool> IsEmailAlreadyRegistered(string email)
    {
        try
        {
            var usersByEmail = await client
                .Child("Users")
                .OrderBy("Email")
                .EqualTo(email)
                .OnceAsync<UserAuthData>();

            // If any result is returned, the email is already registered
            return usersByEmail.Any();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            
        }

        return false;
    }

    public async Task<bool> IsUserWithIdExist(string recordId)
    {
        return (await GetUserByRecordId(recordId)) != null;
    }

    #region Private

    private async Task<UserAuthData> GetUserByRecordId(string recordId)
    {
        try
        {
            return await client
                .Child("Users")
                .Child(recordId)
                .OnceSingleAsync<UserAuthData>();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return null;
        }
    }
    
    private async Task<UserAuthData> GetUserByEmail(string email)
    {
        try
        {
            return  await client
                .Child("Users")
                .OrderBy("Email")
                .EqualTo(email)
                .OnceSingleAsync<UserAuthData>();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return null;
        }
    }
    
    private async Task<UserAuthData> GetUserByDisplayName(string DisplayName)
    {
        try
        {
            return  await client
                .Child("Users")
                .OrderBy("DisplayName")
                .EqualTo(DisplayName)
                .OnceSingleAsync<UserAuthData>();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return null;
        }
    }
    
    private async Task<UserAuthData> GetUserById(string Id)
    {
        try
        {
            return  await client
                .Child("Users")
                .OrderBy("Id")
                .EqualTo(Id)
                .OnceSingleAsync<UserAuthData>();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return null;
        }
    }

    #endregion
}