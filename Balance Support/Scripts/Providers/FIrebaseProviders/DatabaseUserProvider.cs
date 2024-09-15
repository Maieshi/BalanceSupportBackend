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

    public async Task<UserAuthData?> GetUser(string userCred)
    {
        //TODO: перенести UserAuthData в records и сделать все GetUserBy... через FirebaseObject
        var userByDisplayName = await GetUserByDisplayName(userCred);

        if (userByDisplayName != null)
        {
            return userByDisplayName.Object;
        }

        var userByEmail = await GetUserByEmail(userCred);
        if (userByEmail != null)
        {
            return userByEmail.Object;
        }

        var userById = await GetUserById(userCred);
        if (userById != null)
        {
            return userById.Object;
        }

        var userByRecordId = await GetUserByRecordId(userCred);
        if (userByRecordId != null)
        {
            return userByRecordId.Object;
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

    public async Task<bool> IsUserWithIdExist(string userId)
    {
        return (await GetUserById(userId)) != null;
    }

    #region Private

    private async Task<FirebaseObject<UserAuthData>?> GetUserByRecordId(string recordId)
        => (await client
            .Child("Users")
            .Child(recordId)
            .OnceAsync<UserAuthData>()).FirstOrDefault();


    private async Task<FirebaseObject<UserAuthData>?> GetUserByEmail(string email)
        => (await client
            .Child("Users")
            .OrderBy("Email")
            .EqualTo(email)
            .OnceAsync<UserAuthData>()).FirstOrDefault();


    private async Task<FirebaseObject<UserAuthData>?> GetUserByDisplayName(string DisplayName)
        => (await client
            .Child("Users")
            .OrderBy("DisplayName")
            .EqualTo(DisplayName)
            .OnceAsync<UserAuthData>()).FirstOrDefault();


    private async Task<FirebaseObject<UserAuthData>?> GetUserById(string Id)
        => (await client
            .Child("Users")
            .OrderBy("Id")
            .EqualTo(Id)
            .OnceAsync<UserAuthData>()).FirstOrDefault();

    #endregion
}