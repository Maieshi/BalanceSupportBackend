using System.ComponentModel.DataAnnotations;
using Balance_Support.Interfaces;
using Firebase.Auth;
using FirebaseAuthException = FirebaseAdmin.Auth.FirebaseAuthException;
using Balance_Support.SerializationClasses;
using Balance_Support.DataClasses;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using Balance_Support.Scripts.Extensions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using User = Balance_Support.DataClasses.DatabaseEntities.User;

namespace Balance_Support;

public class AuthUserProvider : IAuthUserProvider
{
    private IFirebaseAuthProvider provider;

    private EmailAddressAttribute emailAttribute;

    private IDatabaseUserProvider databaseUserProvider;

    private IHttpContextAccessor httpContextAccessor;

    public AuthUserProvider(IDatabaseUserProvider databaseUserProvider, IFirebaseAuthProvider provider,
        IHttpContextAccessor httpContextAccessor)
    {
        this.provider = provider;
        emailAttribute = new EmailAddressAttribute();
        this.databaseUserProvider = databaseUserProvider;
        this.httpContextAccessor = httpContextAccessor;
    }

    public async Task<IResult> RegisterNewUser(string username, string email, string pasword)
    {
        //if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(pasword) ||
        //    emailAttribute.IsValid(email) == false)
        //{
        //    return
        //        Results.BadRequest(
        //            $"Invalid email:{email}  username:{username} or password:{pasword}. Check your data");
            
        //}
        
        if (await databaseUserProvider.IsEmailAlreadyRegistered(email)||await databaseUserProvider.IsUserWithUsernameExist(username))
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

        var response = await databaseUserProvider.CreateUserAsync(new User()
            { Id = link.User.LocalId, Email = link.User.Email, DisplayName = link.User.DisplayName });
        if (response.IsSuccess == false)
        {
            //TODO сделать удаление пользователя из базы данных
            Results.Problem(statusCode: 500,
                title: "An error occurred while pushing user to database ", detail: response.ErrorMessage);
        }
                
        return (Results.Created($"/Users/{link.User.LocalId}", link.User));
    }

    public async Task<IResult> LogInUser(HttpContext context, string userCred, string password,
        LoginDeviceType deviceType)
    {
        //if (string.IsNullOrEmpty(userCred) || string.IsNullOrEmpty(password))
        //{
        //    return Results.BadRequest($"Invalid email: {userCred} or password: {password}. Check your data.");
        //}

        try
        {
            var user = await databaseUserProvider.GetUser(userCred);
            if (user == null)
                return Results.Problem(detail: "Cannot find user in database", statusCode: 500,
                    title: "User not found");
            string userEmail = emailAttribute.IsValid(userCred) ? userCred : user.Email;

            var authLink = await provider.SignInWithEmailAndPasswordAsync(userEmail, password);

            // Manage claims-based session
            await SignInUserV2(userCred, password, context);

            return Results.Ok(new { user.Id, authLink.FirebaseToken });
        }
        catch (Exception ex)
        {
            return Results.Problem(detail: ex.Message, statusCode: 500, title: "An error occurred while logging in");
        }
    }

    public async Task<IResult> LogOutUser()
    {
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
            ExpiresUtc = DateTime.UtcNow.Add(GetSessionTimeout(deviceType)),
            AllowRefresh = true,
            RedirectUri = "/",
            Items =
            {
                { ".AuthScheme", CookieAuthenticationDefaults.AuthenticationScheme }
            }
        };

        await httpContextAccessor.HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(claimsIdentity),
            authProperties);
    }

    public async Task SignInUserV2(string username, string password, HttpContext httpContext)
    {
        // Create the claims for the user
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, username),
            new Claim(ClaimTypes.Role, "User") // Add roles if needed
        };

        // Create the identity and principal
        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

        // Sign in the user and create the authentication cookie
        await httpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal,
            new AuthenticationProperties
            {
                IsPersistent = true, // Make the session persistent (i.e., cookie will persist across sessions)
                ExpiresUtc = DateTime.UtcNow.AddDays(7) // Set cookie expiration time
            });
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