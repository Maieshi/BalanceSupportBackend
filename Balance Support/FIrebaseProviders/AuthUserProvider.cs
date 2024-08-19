using System.ComponentModel.DataAnnotations;

using Google.Apis.Auth;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Web;
using Balance_Support.Interfaces;
using Firebase.Auth;
using FirebaseAdmin;
using FirebaseAdmin.Auth;
using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore.Http.Abstractions;
using FirebaseAuthException = FirebaseAdmin.Auth.FirebaseAuthException;
using FirebaseAdminAuth = FirebaseAdmin.Auth.FirebaseAuth;
using FirebaseAuth = Firebase.Auth.FirebaseAuth;

using Balance_Support.SerializationClasses;
using Newtonsoft.Json;

namespace Balance_Support;

public class AuthUserProvider: IAuthUserProvider
{
    
    private FirebaseAdminAuth admin =>FirebaseAdminAuth.DefaultInstance;
    
    private IFirebaseAuthProvider provider;
    // private FirebaseAuthProvider provider;

    private EmailAddressAttribute emailAttribute;

    // private const string apiKey = "AIzaSyBQ6MmEOw3kCEH0s56Ux5KwtdVWs_0jdOY";
    //
    // private const string bucketId = "balance-support-b9da3.appspot.com";

    private List<FirebaseAuthLink> loggedInUsers;

    private DatabaseUserProvider _databaseUserProvider;


    public AuthUserProvider()
    {
        
        // var app = FirebaseApp.Create(new AppOptions()
        // {
        //     Credential = GoogleCredential.FromFile(@"C:\Projects\Asp. Net\Balance Support\Balance Support\balance-support-431615-0a30add1ec30.json"),
        // });
        //
        var firebseAuthApiKey = JsonConvert.DeserializeObject<FirebaseAuthApiKey>(File.ReadAllText(Path.Combine(PathStorage.FirebaseConfigsPath, PathStorage.FirebaseAuthApiKey)));
        provider = new Firebase.Auth.FirebaseAuthProvider(new FirebaseConfig(firebseAuthApiKey.ApiKey));
        emailAttribute = new EmailAddressAttribute();
        loggedInUsers = new List<FirebaseAuthLink>();
        // _databaseUserProvider = new DatabaseUserProvider();

        Test();

    }

    public AuthUserProvider(DatabaseUserProvider databaseUserProvider, IFirebaseAuthProvider provider)
    {
        this.provider = provider;
        emailAttribute = new EmailAddressAttribute();
        loggedInUsers = new List<FirebaseAuthLink>();
        _databaseUserProvider = databaseUserProvider;

        // Test();
    }

    async void Test()
    {
        // var newUser1 = await RegisterNewUser("testuser1", "testuser1@gmail.com", "123123123");
        // var newUser2 = await RegisterNewUser("testuser2", "testuser2@gmail.com", "123123123");
        // databaseProvider.TryCreateNewUser(new UserAuthData() { Id = newUser1.LocalId, Email = newUser1.Item2.User.Email, DisplayName = newUser1.Item2.User.DisplayName });
        // databaseProvider.TryCreateNewUser(new UserAuthData() { Id = newUser2.User.LocalId, Email = newUser2.Item2.User.Email, DisplayName = newUser2.Item2.User.DisplayName });

        // Debug.Print(databaseProvider.IsUserExists("testuser1").ToString());
        // Debug.Print(databaseProvider.IsUserExists("testuser2").ToString());
        // Debug.Print(databaseProvider.IsUserExists("testuser1@gmail.com").ToString());
        // Debug.Print(databaseProvider.IsUserExists("testuser2@gmail.com").ToString());
        // Debug.Print(databaseProvider.IsUserExists("asdfasdf").ToString());
        // var authLink = await provider.SignInWithEmailAndPasswordAsync("testuser1@gmail.com", "123123123");
        // var user = await provider.GetUserAsync("testuser1@gmail.com");
        // var a = authLink;
        var res = await LogInUser("-O3Yly5O5gBeLic7_rAW","testuser1@gmail.com", "123123123", LoginDeviceType.Mobile);
        //Debug.Print(res.Item2.FirebaseToken);
    }

    public async Task<IResult> RegisterNewUser(string username, string email, string pasword)
    {
        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(pasword) ||
            emailAttribute.IsValid(email) == false)
        {
            return
                Results.BadRequest(
                    $"Invalid email:{email}  username:{username} or password:{pasword}. Check your data");
        }
        
        if (_databaseUserProvider.TryGetUser(email, out var user))
            return Results.BadRequest("User already exists");

        FirebaseAuthLink link;

        try
        {
            // var newUser = await admin.CreateUserAsync(new UserRecordArgs(){Email = email, Password = pasword, DisplayName = username, EmailVerified = true});
            link = await provider.CreateUserWithEmailAndPasswordAsync(email, pasword, username,true);
        }
        catch (FirebaseAuthException ex)
        {
            return
                Results.Problem(detail: ex.Message, statusCode: 500,
                    title: "An error occurred while creating the user");
        }
        
        var newUser = await _databaseUserProvider.CreateNewUserAsync(new UserAuthData()
            { Id = link.User.LocalId, Email = link.User.Email, DisplayName = link.User.DisplayName });
        if (newUser == String.Empty) return
            Results.Problem( statusCode: 500,
                title: "An error occurred while pushing user to database ");
        return (Results.Created($"/Users/{newUser}", newUser));
    }

    public async Task<IResult> LogInUser(string userRecordId,string userCred, string password,LoginDeviceType deviceType)
    {
        if (string.IsNullOrEmpty(userCred) || string.IsNullOrEmpty(password))
        {
            return (Results.BadRequest($"Invalid email: {userCred} or password: {password}. Check your data."));
        }

        FirebaseAuthLink authLink;
        
        string userEmail = default;

        if (emailAttribute.IsValid(userCred) == false)
        {
            if (string.IsNullOrEmpty(userRecordId))
            {
                
                if (_databaseUserProvider.TryGetUser(userCred, out var user))
                {
                    userEmail = user.Email;
                }
                else
                {
                    return Results.Problem(detail: "cannot find user in database", statusCode: 500,
                        title: "Cannot find user by username");
                }
            }
            else
            {
                if (_databaseUserProvider.TryGetUserByRecordId(userRecordId, out var user))
                {
                    userEmail = user.Email;
                }
                else
                {
                    return Results.Problem(detail: "cannot find user in database", statusCode: 500,
                        title: "Cannot find user by username");
                }
            }
           
            // try
            // {
            //     var token = await admin.CreateCustomTokenAsync((await admin.GetUserByEmailAsync(userCred)).Uid);
            //     
            //     authLink = await provider.SignInWithCustomTokenAsync(token);
            // }
            // catch (Exception ex)
            // {
            //     return Results.Problem(detail: ex.Message, statusCode: 500,
            //             title: "An error occurred while login through username");
            // }
        }
        else userEmail = userCred;
         
            try
            {
                authLink = await provider.SignInWithEmailAndPasswordAsync(userEmail, password);
            
                return Results.Ok(new
                {
                    User = new
                    {
                       userEmail
                    },
                    
                });
            }
            catch (Exception ex)
            {
                return
                    Results.Problem(detail: ex.Message, statusCode: 500, title: "An error occurred while login through email");
            }
        

        if (authLink != default)
        {
            var user = authLink.User; // Get the authenticated user
            var token = authLink.FirebaseToken; // Get the ID token

            // SignInUser(wrapper ,user.Email, token, false);

            // Return the user info and token
            return Results.Ok(new
            {
                User = new
                {
                    user.Email,
                    user.DisplayName,
                    user.LocalId // User ID
                },
                Token = token
            });
        }

        return Results.Problem(detail: "Dafuck!!", statusCode: 500, title: "AuthLink is default");
    }

    public Task<IResult> LogOutUser(string userRecordId, string userCred, string password, LoginDeviceType deviceType)
    {
        throw new NotImplementedException();
    }

    // public void SignInUser(HttpRequestWrapper wrapper,string email, string token, bool isPersistent)
    // {
    //     var claims = new List<Claim>();
    //
    //     try
    //     {
    //         // Setting
    //         claims.Add(new Claim(ClaimTypes.Email, email));
    //         claims.Add(new Claim(ClaimTypes.Authentication, token));
    //         var claimIdenties = new ClaimsIdentity(claims, DefaultAuthenticationTypes.ApplicationCookie);
    //         var ctx = wrapper.GetOwinContext();
    //         var authenticationManager = ctx.Authentication;
    //         // Sign In.
    //         authenticationManager.SignIn(new AuthenticationProperties() { IsPersistent = isPersistent }, claimIdenties);
    //     }
    //     catch (Exception ex)
    //     {
    //         // Info
    //         throw new Exception("Failed to sign in user",ex);
    //     }
    // }

    public async Task<(IResult, FirebaseAuthLink?)> SignOutUser(string userCred)
    {
        if (string.IsNullOrEmpty(userCred))
        {
            return (Results.BadRequest($"Invalid email: {userCred}. Check your data."),
                default);
        }

        FirebaseAuthLink? authLink = default;

        if (emailAttribute.IsValid(userCred) == false)
        {
            try
            {
                var link = loggedInUsers.FirstOrDefault(x => x.User.DisplayName == userCred);
                link.RefreshUserDetails();
            }
            catch (Exception ex)
            {
                return ((
                    Results.Problem(detail: ex.Message, statusCode: 500,
                        title: "An error occurred while login through username"), default));
            }
        }
        else
        {
            try
            {
                var link = loggedInUsers.FirstOrDefault(x => x.User.Email == userCred);
                link.RefreshUserDetails();
            }
            catch (Exception ex)
            {
                return
                    (Results.Problem(detail: ex.Message, statusCode: 500, title: "An error occurred while login through email"),
                        default);
            }
        }

        if (authLink != default)
        {
            var user = authLink.User; // Get the authenticated user
            var idToken = authLink.FirebaseToken; // Get the ID token

            // Return the user info and token
            return (Results.Ok(new
            {
                User = new
                {
                    user.Email,
                    user.DisplayName,
                    user.LocalId // User ID
                },
                Token = idToken
            }), authLink);
        }

        return (Results.Problem(detail: "Dafuck!!", statusCode: 500, title: "AuthLink is default"),
            default);
    }
}

public enum LoginDeviceType
{
    Mobile,
    Desktop
}