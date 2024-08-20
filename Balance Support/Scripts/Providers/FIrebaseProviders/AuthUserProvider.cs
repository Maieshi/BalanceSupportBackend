using System.ComponentModel.DataAnnotations;
using Balance_Support.Interfaces;
using Firebase.Auth;
using FirebaseAuthException = FirebaseAdmin.Auth.FirebaseAuthException;
using Balance_Support.SerializationClasses;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using Balance_Support.Scripts.Extensions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
namespace Balance_Support;

public class AuthUserProvider : IAuthUserProvider
{
    private IFirebaseAuthProvider provider;

    private EmailAddressAttribute emailAttribute;

    private IDatabaseUserProvider databaseUserProvider;

    private IHttpContextAccessor httpContextAccessor;

    public AuthUserProvider(IDatabaseUserProvider databaseUserProvider, IFirebaseAuthProvider provider)
    {
        this.provider = provider;
        emailAttribute = new EmailAddressAttribute();
        this.databaseUserProvider = databaseUserProvider;
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

        if (databaseUserProvider.TryGetUser(email, out var user))
            return Results.BadRequest("User already exists");

        FirebaseAuthLink link;

        try
        {
            link = await provider.CreateUserWithEmailAndPasswordAsync(email, pasword, username, true);
        }
        catch (FirebaseAuthException ex)
        {
            return
                Results.Problem(detail: ex.Message, statusCode: 500,
                    title: "An error occurred while creating the user");
        }

        var newUser = await databaseUserProvider.CreateNewUserAsync(new UserAuthData()
            { Id = link.User.LocalId, Email = link.User.Email, DisplayName = link.User.DisplayName });
        if (newUser == String.Empty)
            return
                Results.Problem(statusCode: 500,
                    title: "An error occurred while pushing user to database ");
        return (Results.Created($"/Users/{newUser}", newUser));
    }

    public async Task<IResult> LogInUser(string userRecordId, string userCred, string password,
        LoginDeviceType deviceType)
    {
        if (string.IsNullOrEmpty(userCred) || string.IsNullOrEmpty(password))
        {
            return Results.BadRequest($"Invalid email: {userCred} or password: {password}. Check your data.");
        }

        try
        {
            var userEmail = ResolveUserEmail(userRecordId, userCred);
            if (userEmail == null)
                return Results.Problem(detail: "Cannot find user in database", statusCode: 500, title: "User not found");

            var authLink = await provider.SignInWithEmailAndPasswordAsync(userEmail, password);

            // Manage claims-based session
            await SignInUser(authLink, deviceType);

            return Results.Ok(new
            {
                User = new { userEmail },
                Token = authLink.FirebaseToken // Example: Returning token to client
            });
        }
        catch (Exception ex)
        {
            return Results.Problem(detail: ex.Message, statusCode: 500, title: "An error occurred while logging in");
        }
    }

    public async Task<IResult> LogOutUser()
    {
        if (!httpContextAccessor.HttpContext.IsUserAuthorized())
        {
            return Results.Unauthorized();
        }

        await httpContextAccessor.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return Results.Ok("User has been logged out successfully.");
    }
    
    private async Task SignInUser(FirebaseAuthLink authLink, LoginDeviceType deviceType)
    {
        var expirationTime = DateTime.UtcNow.Add(GetSessionTimeout(deviceType));
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, authLink.User.LocalId),
            new Claim(ClaimTypes.Email, authLink.User.Email),
            new Claim("FirebaseToken", authLink.FirebaseToken),
            new Claim("SessionStartTime", DateTime.UtcNow.ToString("o")), // ISO 8601 format
            new Claim("DeviceType", deviceType.ToString()),
            new Claim("ExpiresUtc", expirationTime.ToString("o")) 
        };

        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

        var authProperties = new AuthenticationProperties
        {
            IsPersistent = true, // Keep the user logged in across sessions
            ExpiresUtc = DateTime.UtcNow.Add(GetSessionTimeout(deviceType))
        };

        await httpContextAccessor.HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(claimsIdentity),
            authProperties);
    }

    private string ResolveUserEmail(string userRecordId, string userCred)
    {
        if (emailAttribute.IsValid(userCred))
            return userCred;

        if (!string.IsNullOrEmpty(userRecordId) &&
            databaseUserProvider.TryGetUserByRecordId(userRecordId, out var user))
            return user.Email;

        if (databaseUserProvider.TryGetUser(userCred, out var userByUsername))
            return userByUsername.Email;

        return null;
    }
    
    private TimeSpan GetSessionTimeout(LoginDeviceType deviceType)
    {
        return deviceType switch
        {
            LoginDeviceType.Mobile => TimeSpan.FromDays(7), // Longer session for mobile
            LoginDeviceType.Desktop => TimeSpan.FromHours(24), // Shorter session for desktop
            _ => TimeSpan.FromMinutes(30),
        };
    }
}

public enum LoginDeviceType
{
    Mobile,
    Desktop
}

// if (string.IsNullOrEmpty(userCred))
// {
//     return (Results.BadRequest($"Invalid email: {userCred}. Check your data."),
//         default);
// }
//
// FirebaseAuthLink? authLink = default;
//
// if (emailAttribute.IsValid(userCred) == false)
// {
//     try
//     {
//         var link = loggedInUsers.FirstOrDefault(x => x.User.DisplayName == userCred);
//         link.RefreshUserDetails();
//     }
//     catch (Exception ex)
//     {
//         return ((
//             Results.Problem(detail: ex.Message, statusCode: 500,
//                 title: "An error occurred while login through username"), default));
//     }
// }
// else
// {
//     try
//     {
//         var link = loggedInUsers.FirstOrDefault(x => x.User.Email == userCred);
//         link.RefreshUserDetails();
//     }
//     catch (Exception ex)
//     {
//         return
//             (Results.Problem(detail: ex.Message, statusCode: 500, title: "An error occurred while login through email"),
//                 default);
//     }
// }
//
// if (authLink != default)
// {
//     var user = authLink.User; // Get the authenticated user
//     var idToken = authLink.FirebaseToken; // Get the ID token
//
//     // Return the user info and token
//     return (Results.Ok(new
//     {
//         User = new
//         {
//             user.Email,
//             user.DisplayName,
//             user.LocalId // User ID
//         },
//         Token = idToken
//     }), authLink);
// }
//
// return (Results.Problem(detail: "Dafuck!!", statusCode: 500, title: "AuthLink is default"),
//     default);

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

// if (authLink != default)
// {
//     var user = authLink.User; // Get the authenticated user
//     var token = authLink.FirebaseToken; // Get the ID token
//
//     // SignInUser(wrapper ,user.Email, token, false);
//
//     // Return the user info and token
//     return Results.Ok(new
//     {
//         User = new
//         {
//             user.Email,
//             user.DisplayName,
//             user.LocalId // User ID
//         },
//         Token = token
//     });
//     return Results.Problem(detail: "Dafuck!!", statusCode: 500, title: "AuthLink is default");

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

// async void Test()
// {
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
// var res = await LogInUser("-O3Yly5O5gBeLic7_rAW","testuser1@gmail.com", "123123123", LoginDeviceType.Mobile);
//Debug.Print(res.Item2.FirebaseToken);
// }

// public AuthUserProvider()
// {
//     
//     // var app = FirebaseApp.Create(new AppOptions()
//     // {
//     //     Credential = GoogleCredential.FromFile(@"C:\Projects\Asp. Net\Balance Support\Balance Support\balance-support-431615-0a30add1ec30.json"),
//     // });
//     //
//     var firebseAuthApiKey = JsonConvert.DeserializeObject<FirebaseAuthApiKey>(File.ReadAllText(Path.Combine(PathStorage.FirebaseConfigsPath, PathStorage.FirebaseAuthApiKey)));
//     provider = new Firebase.Auth.FirebaseAuthProvider(new FirebaseConfig(firebseAuthApiKey.ApiKey));
//     emailAttribute = new EmailAddressAttribute();
//     loggedInUsers = new List<FirebaseAuthLink>();
//     // _databaseUserProvider = new DatabaseUserProvider();
//
//     Test();
//
// }

// private const string apiKey = "AIzaSyBQ6MmEOw3kCEH0s56Ux5KwtdVWs_0jdOY";
//
// private const string bucketId = "balance-support-b9da3.appspot.com";

// private List<FirebaseAuthLink> loggedInUsers;

// public async Task<(IResult, FirebaseAuthLink?)> SignOutUser(string userCred)
// {
// logout implementation
// }