using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Security.Claims;
using Balance_Support.DataClasses.Records.UserData;
using Balance_Support.Scripts.Controllers.Interfaces;
using Balance_Support.Scripts.Database.Providers.Interfaces.User;
using Balance_Support.Scripts.Database.Providers.Interfaces.UserSettings;
using Firebase.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using FirebaseAuthException = FirebaseAdmin.Auth.FirebaseAuthException;

namespace Balance_Support.Scripts.Controllers;

public class UserController: IUserController 
{
    // private readonly IDatabaseUserSettingsProvider databaseUserSettingsProvider;
    private IFirebaseAuthProvider firebaseAuthProvider;
    //
    private EmailAddressAttribute emailAttribute;
    //
    // private IDatabaseUserProvider databaseUserProvider;
    //
    // private IHttpContextAccessor httpContextAccessor;

    public UserController(IFirebaseAuthProvider firebaseAuthProvider)
    {
        this.firebaseAuthProvider = firebaseAuthProvider;
        emailAttribute = new EmailAddressAttribute();
    }

    public async Task<IResult> RegisterNewUser(UserRegisterRequest userRegisterRequest, ICheckEmailAlreadyRegistered checkEmailRegistered, ICheckUserWithUsernameExist checkUserRegistered, IRegisterUser registerUser, ICreateUserSettings createUserSettings )
    {
        if (await checkEmailRegistered.CheckEmail(userRegisterRequest.Email)||await checkUserRegistered.CheckUsername(userRegisterRequest.DisplayName))
            return Results.BadRequest("User already exists");

        FirebaseAuthLink link;

        try
        {
            link = await firebaseAuthProvider.CreateUserWithEmailAndPasswordAsync(userRegisterRequest.Email,userRegisterRequest.Password,userRegisterRequest.DisplayName, true);
        }
        catch (FirebaseAuthException ex)
        {
            return
                Results.Problem(detail: ex.Message, statusCode: 500,
                    title: "An error occurred while creating the user");
        }

        try
        {
            await registerUser.RegisterUser(link.User.LocalId, link.User.Email, link.User.DisplayName);
        }
        catch (Exception e)
        {
            //TODO сделать удаление пользователя из базы данных
            Results.Problem(detail: e.Message, statusCode: 500,
                title: "An error occurred while pushing user to database");   
        }

        var isSettingsCreated =await createUserSettings.CreateUserSetting(link.User.LocalId);
        
        if(isSettingsCreated==null)
            return Results.Problem(statusCode: 500,
                title: "An error occurred while pushing user settings to database ");
                
        return Results.Created($"/Users/{link.User.LocalId}", link.User.DisplayName);
    }

    public async Task<IResult> LogInUser(UserLoginRequest loginRequest,HttpContext context, LoginDeviceType deviceType, IGetUser getUser)
    {
        try
        {
            var user = await getUser.GetUser(loginRequest.UserCred);
            if (user == null)
                return Results.Problem(detail: "Cannot find user in database", statusCode: 500,
                    title: "User not found");
            string userEmail = emailAttribute.IsValid(loginRequest.UserCred) ? loginRequest.UserCred : user.Email;

            var authLink = await firebaseAuthProvider.SignInWithEmailAndPasswordAsync(userEmail, loginRequest.Password);

            // Manage claims-based session
            await SignInUserV2(loginRequest.UserCred, loginRequest.Password, context);

            return Results.Ok(new { user.Id,user.DisplayName ,authLink.FirebaseToken });
        }
        catch (Exception ex)
        {
            return Results.Problem(detail: ex.Message, statusCode: 500, title: "An error occurred while logging in");
        }
    }

    public async Task<IResult> LogOutUser(HttpContext context)
    {
        await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return Results.Ok("User has been logged out successfully.");
    }

    private async Task SignInUser(FirebaseAuthLink authLink, LoginDeviceType deviceType,HttpContext context)
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

        await context.SignInAsync(
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
        
        Debug.Print("aaa");
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
