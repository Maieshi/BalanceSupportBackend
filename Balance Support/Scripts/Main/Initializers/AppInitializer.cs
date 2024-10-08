using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Balance_Support.DataClasses.Records.AccountData;
using Balance_Support.DataClasses.Records.NotificationData;
using Balance_Support.DataClasses.Records.UserData;
using Balance_Support.DataClasses.Validators;
using Balance_Support.Scripts.Extensions;
using Balance_Support.Scripts.Providers;
using Balance_Support.Scripts.Providers.Interfaces;
using Balance_Support.Scripts.Validators;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Owin;
using Microsoft.Owin;
using Owin;

namespace Balance_Support.Scripts.Main.Initializers;

public static class AppInitializer
{
    public static async Task Initialize(WebApplication app)
    {
        app.UseCors("AllowSpecificOrigin");
        app.UseHttpsRedirection();
        app.UseSession();
        
        app.UseAuthentication();
        app.UseAuthorization();


        var todos = new List<ToDo>();

        #region RequestsMapping

        #region testovaya huinya

        app.MapGet("/", () => "Hello World!");
        app.MapGet("/GetContext", async (HttpContext context) =>
        {
            var result = await GetHttpContextLog(context);
            return result; // Properly returning the result
        });


        app.MapPost("/todos", (ToDo todo) =>
        {
            todos.Add(todo);
            return TypedResults.Created($"/todos/{todo.id}", todo);
        });
        app.MapGet("/GetDatabase", async (ApplicationDbContext context) => TypedResults.Ok(new
        {
            Users = UserDto.CreateDtos(await context.Users.ToListAsync()),
            UserSettings = UserSettingsDto.CreateDtos(await context.UserSettings.ToListAsync()),
            Accounts = AccountDto.CreateDtos(await context.Accounts.ToListAsync()),
            Transactions = TransactionDto.CreateDtos(await context.Transactions.ToListAsync()),
            UserTokens = UserTokenDto.CreateDtos(await context.UserTokens.ToListAsync())
        }));

        #endregion

        #region User

        app.MapPost("/Desktop/User/Register",
            async ([FromBody] UserRegisterRequest userRegisterRequest, IAuthUserProvider authProvider) =>
            (await ResultContainer
                .Start()
                .Validate<UserRegisterRequest, UserRegisterRequestValidator>(userRegisterRequest)
                .ProcessAsync(
                    async () =>
                        await authProvider.RegisterNewUser(userRegisterRequest.DisplayName, userRegisterRequest.Email,
                            userRegisterRequest.Password)))
            .GetResult());

        app.MapPost("/Mobile/User/Login",
            async ([FromBody] UserLoginRequest userSignData, IAuthUserProvider authProvider, HttpContext context) =>
            (await ResultContainer
                .Start()
                .Validate<UserLoginRequest, UserLoginRequestValidator>(userSignData)
                .ProcessAsync(
                    async () =>
                        await authProvider.LogInUser(context,
                            userSignData.UserCred,
                            userSignData.Password,
                            LoginDeviceType.Mobile)))
            .GetResult()
        );

        app.MapPost("/Desktop/User/Login",
            async ([FromBody] UserLoginRequest userSignData, IAuthUserProvider authProvider, HttpContext context) =>
            (await ResultContainer
                .Start()
                .Validate<UserLoginRequest, UserLoginRequestValidator>(userSignData)
                .ProcessAsync(
                    async () =>
                        await authProvider.LogInUser(context,
                            userSignData.UserCred,
                            userSignData.Password,
                            LoginDeviceType.Desktop)))
            .GetResult()
        );

        app.MapPost("/User/Logout",
            async (IAuthUserProvider authProvider, HttpContext context) =>
                (await ResultContainer
                    .Start()
                    .Authorize(context)
                    .ProcessAsync(
                        async () =>
                            await authProvider.LogOutUser(context)))
                .GetResult()
        );

        #endregion

        #region UserSettings

        app.MapGet("/Desktop/UserSettings/Get/{userID}",
            async (string userId, IDatabaseUserSettingsProvider provider,
                HttpContext context) =>
            {
                var getRequest = new UserSettingsGetRequest(userId);

                return (await ResultContainer
                        .Start()
                        .Authorize(context)
                        .Validate<UserSettingsGetRequest, UserSettingsGetRequestValidator>(getRequest)
                        .ProcessAsync(
                            async () =>
                                await provider.GetUserSettings(getRequest)
                        ))
                    .GetResult();
            }
        );

        app.MapPost("/Desktop/UserSettings/Update",
            async ([FromBody] UserSettingsUpdateRequest updateRequest, IDatabaseUserSettingsProvider provider,
                    HttpContext context) =>
                (await ResultContainer
                    .Start()
                    .Authorize(context)
                    .Validate<UserSettingsUpdateRequest, UserSettingsUpdateRequestValidator>(updateRequest)
                    .ProcessAsync(
                        async () =>
                            await provider.UpdateUserSettings(updateRequest)
                    ))
                .GetResult()
        );

        #endregion

        #region Account

        app.MapPost("/Desktop/Account/Register", async ([FromBody] AccountRegisterRequest deviceRegisterData,
                IDatabaseAccountProvider deviceProvider, HttpContext context) =>
            (await ResultContainer
                .Start()
                .Validate<AccountRegisterRequest, DeviceRegisterRequestValidator>(deviceRegisterData)
                .Authorize(context)
                .ProcessAsync(async () => await deviceProvider.RegisterAccount(deviceRegisterData)))
            .GetResult()
        );

        app.MapPost("/Desktop/Account/Update", async ([FromBody] AccountUpdateRequest deviceRegisterData,
                IDatabaseAccountProvider deviceProvider, HttpContext context) =>
            (await ResultContainer
                .Start()
                .Validate<AccountUpdateRequest, DeviceUpdateRequestValidator>(deviceRegisterData)
                .Authorize(context)
                .ProcessAsync(async () => await deviceProvider.UpdateAccount(deviceRegisterData)))
            .GetResult()
        );

        app.MapPost("/Desktop/Account/Delete", async ([FromBody] AccountDeleteRequest deviceRegisterData,
                IDatabaseAccountProvider deviceProvider, HttpContext context) =>
            (await ResultContainer
                .Start()
                .Validate<AccountDeleteRequest, DeviceDeleteRequestValidator>(deviceRegisterData)
                .Authorize(context)
                .ProcessAsync(async () => await deviceProvider.DeleteAccount(deviceRegisterData)))
            .GetResult()
        );

        app.MapGet("/Mobile/Account/GetForDevice/{userId}/{accountGroup:int}/{deviceId:int}",
            async (string userId, int accountGroup, int deviceId,
                IDatabaseAccountProvider deviceProvider, HttpContext context) =>
            {
                // Create the request object from the URL parameters 
                var deviceGetRequestData = new AccountGetForDeviceRequest(userId, accountGroup, deviceId);

                // Process the request using the same logic
                return (await ResultContainer
                        .Start()
                        .Validate<AccountGetForDeviceRequest, AccountGetForDeviceRequestValidator>(deviceGetRequestData)
                        .Authorize(context)
                        .ProcessAsync(async () => await deviceProvider.GetAccountsForDevice(deviceGetRequestData)))
                    .GetResult();
            });

        app.MapGet("/Desktop/Account/GetAllForUser/{userId}",
            async (string userId, IDatabaseAccountProvider deviceProvider, HttpContext context) =>
            {
                // Create the request object from the URL parameter
                var getAllForUserRequest = new AccountGetAllForUserRequest(userId);

                // Process the request using the same logic
                return (await ResultContainer
                        .Start()
                        .Validate<AccountGetAllForUserRequest, AccountGetAllForUserRequestValidator>(
                            getAllForUserRequest)
                        .Authorize(context)
                        .ProcessAsync(async () => await deviceProvider.GetAllAccountsForUser(getAllForUserRequest)))
                    .GetResult();
            });

        #endregion

        #region UserToken

        app.MapPost("/Desktop/UserToken/Set", async ([FromBody] SetUserTokenRequest userTokenRequest,
                ICloudMessagingProvider cloudMessagingProvider, HttpContext context) =>
            (await ResultContainer
                .Start()
                .Validate<SetUserTokenRequest, SetUserTokenRequestValidator>(userTokenRequest)
                .Authorize(context)
                .ProcessAsync(async () => await cloudMessagingProvider.SetUserToken(userTokenRequest)))
            .GetResult()
        );

        app.MapPost("/Desktop/UserToken/Delete", async ([FromBody] DeleteUserTokenRequest userTokenRequest,
                ICloudMessagingProvider cloudMessagingProvider, HttpContext context) =>
            (await ResultContainer
                .Start()
                .Validate<DeleteUserTokenRequest, UserTokenDeleteRequestValidator>(userTokenRequest)
                .Authorize(context)
                .ProcessAsync(async () => await cloudMessagingProvider.DeleteUserToken(userTokenRequest)))
            .GetResult()
        );

        #endregion

        #region Transaction

        app.MapPost("/Mobile/Transaction/HandleNew", async (
                [FromBody] NotificationHandleRequest handleNotificationRequest,
                INotificationHandler notificationHandler, HttpContext context) =>
            (await ResultContainer
                .Start()
                .Validate<NotificationHandleRequest, NotificationHandleRequestValidator>(handleNotificationRequest)
                .Authorize(context)
                .ProcessAsync(async () => await notificationHandler.HandleNotification(handleNotificationRequest)))
            .GetResult()
        );


        app.MapPost("/Desktop/Transaction/GetMessages", async (
                [FromBody] MessagesGetRequest getMessagesRequest,
                IDatabaseTransactionProvider databaseTransactionProvider, HttpContext context) =>
            (await ResultContainer
                .Start()
                .Validate<MessagesGetRequest, MessagesGetRequestValidator>(getMessagesRequest)
                .Authorize(context)
                .ProcessAsync(async () => await databaseTransactionProvider.GetMessages(getMessagesRequest)))
            .GetResult()
        );

        #endregion

        #endregion


//TODO: create new endpoint filtration and validation
//TODO: try to rebuild project to mvc
//TODO: split notification handling to parser(mb static) that returns transaction and transaction handler
//TODO: try to upgrade architecture and make the code less cohesive
    }


    public static async Task<IResult> GetHttpContextLog(HttpContext context)
    {
        // Log request details
        var request = context.Request;
        var headers = request.Headers;
        var queryParams = request.Query;
        var method = request.Method;
        var path = request.Path;
        var scheme = request.Scheme;
        var host = request.Host.ToString();
        var protocol = request.Protocol;
        var queryString = request.QueryString.ToString();
        var pathBase = request.PathBase.ToString();
        var contentType = request.ContentType;
        var cookies = request.Cookies;
        var remoteIpAddress = context.Connection.RemoteIpAddress?.ToString();
        string requestBody = "";

        // Log Request Body (for methods like POST/PUT)
        if (request.ContentLength > 0 &&
            (request.Method == HttpMethods.Post || request.Method == HttpMethods.Put))
        {
            request.EnableBuffering(); // Allows re-reading the body
            using (var reader = new StreamReader(request.Body, Encoding.UTF8, leaveOpen: true))
            {
                requestBody = await reader.ReadToEndAsync();
                request.Body.Position = 0; // Reset the body stream position
            }
        }

        // Log user information
        var user = context.User;
        string userString = string.Empty;
        try
        {
            userString = JsonConvert.SerializeObject(user);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
        var identity = user.Identity;
        var isAuthenticated = user.Identity?.IsAuthenticated ?? false;
        var userName = user.Identity?.Name;
        var claims = user.Claims.Select(c => new { c.Type, c.Value });

        // Decode JWT token if present in Authorization header
        var jwtToken = request.Headers["Authorization"].ToString().Replace("Bearer ", "");
        var decodedClaims = DecodeJwtToken(jwtToken);

        // Log the response status code (response code might not be available at this point)
        var responseStatusCode = context.Response.StatusCode;

        // Return all relevant details as an object
        return TypedResults.Ok(new
        {
            Method = method,
            Path = path,
            Scheme = scheme,
            Host = host,
            Protocol = protocol,
            QueryParams = queryParams.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ToString()),
            QueryString = queryString,
            PathBase = pathBase,
            ContentType = contentType,
            RemoteIpAddress = remoteIpAddress,
            Cookies = cookies.ToDictionary(c => c.Key, c => c.Value),
            Headers = headers.ToDictionary(h => h.Key, h => h.Value.ToString()),
            RequestBody = requestBody,
            ResponseStatusCode = responseStatusCode,
           
            IsAuthenticated = isAuthenticated,
            UserName = userName,
            Claims = claims.ToList(),
            DecodedClaims = decodedClaims,
            User = userString
        });
    }

// Method to decode JWT token and return claims
    private static List<ClaimInfo> DecodeJwtToken(string token)
    {
        var claimsList = new List<ClaimInfo>();
        if (!string.IsNullOrEmpty(token))
        {
            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadToken(token) as JwtSecurityToken;

            if (jsonToken != null)
            {
                claimsList = jsonToken.Claims.Select(c => new ClaimInfo { Type = c.Type, Value = c.Value }).ToList();
            }
        }

        return claimsList;
    }

// ClaimInfo class definition
    public class ClaimInfo
    {
        public string Type { get; set; }
        public string Value { get; set; }
    }
}

public record ToDo(int id, string name, bool isComplited);