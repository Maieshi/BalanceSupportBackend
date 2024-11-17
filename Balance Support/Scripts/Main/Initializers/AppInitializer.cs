using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Balance_Support.DataClasses.Records.AccountData;
using Balance_Support.DataClasses.Records.NotificationData;
using Balance_Support.DataClasses.Records.UserData;
using Balance_Support.DataClasses.Validators;
using Balance_Support.Scripts.Controllers;
using Balance_Support.Scripts.Controllers.Interfaces;
using Balance_Support.Scripts.Database;
using Balance_Support.Scripts.Database.Providers;
using Balance_Support.Scripts.Database.Providers.Interfaces.Account;
using Balance_Support.Scripts.Database.Providers.Interfaces.Transaction;
using Balance_Support.Scripts.Database.Providers.Interfaces.User;
using Balance_Support.Scripts.Database.Providers.Interfaces.UserSettings;
using Balance_Support.Scripts.Extensions;
using Balance_Support.Scripts.Extensions.EndpointExtensions;
using Balance_Support.Scripts.Parsing;
using Balance_Support.Scripts.WebSockets;
using Balance_Support.Scripts.WebSockets.ConnectionManager;
using Balance_Support.Scripts.WebSockets.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
// using Microsoft.AspNetCore.Owin;
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

        app.MapHub<BaseHub>("/baseHub");
        var todos = new List<ToDo>();

        #region RequestsMapping

        #region testovaya huinya

        app.MapGet("/", () => "Hello World!");
        app.MapGet("/GetContext", async ([FromServices] IHttpContextAccessor httpContextAccessor) =>
        {
            var result = await GetHttpContextLog(httpContextAccessor.HttpContext);
            return result; // Properly returning the result
        });

        app.MapPost("/todos", (ToDo todo) =>
        {
            todos.Add(todo);
            return TypedResults.Created($"/todos/{todo.id}", todo);
        });

        app.MapGet("/GetDatabase", async ([FromServices] ApplicationDbContext context) => TypedResults.Ok(new
        {
            Users = context.Users.ConvertToDtoList(),
            UserSettings = context.UserSettings.ConvertToDtoList(),
            Accounts = context.Accounts.ConvertToDtoList(),
            Transactions = context.Transactions.ConvertToDtoList()
        }));


        app.MapGet("/GetConnections",
            ([FromServices] IConnectionManager connectionManager) =>
                $"connections{connectionManager.Test()} ");

        #endregion

        #region User

        app.MapPost("/Desktop/User/Register",
            async (
                    [FromBody] UserRegisterRequest userRegisterRequest,
                    [FromServices] IUserController controller) =>
                (await ResultContainer
                    .Start()
                    .Validate<UserRegisterRequest, UserRegisterRequestValidator>(userRegisterRequest)
                    .ProcessAsync(
                        async () =>
                            await controller.RegisterNewUser(userRegisterRequest)))
                .GetResult());

        app.MapPost("/Mobile/User/Login",
            async (
                    [FromBody] UserLoginRequest userLoginRequest,
                    [FromServices] IUserController controller,
                    [FromServices] IHttpContextAccessor httpContextAccessor) =>
                (await ResultContainer
                    .Start()
                    .Validate<UserLoginRequest, UserLoginRequestValidator>(userLoginRequest)
                    .ProcessAsync(
                        async () => await controller.LogInUser(userLoginRequest, httpContextAccessor.HttpContext,
                            LoginDeviceType.Mobile)))
                .GetResult()
        );

        app.MapPost("/Desktop/User/Login",
            async (
                    [FromBody] UserLoginRequest userLoginRequest,
                    [FromServices] IUserController controller,
                    [FromServices] IHttpContextAccessor httpContextAccessor) =>
                (await ResultContainer
                    .Start()
                    .Validate<UserLoginRequest, UserLoginRequestValidator>(userLoginRequest)
                    .ProcessAsync(
                        async () => await controller.LogInUser(userLoginRequest, httpContextAccessor.HttpContext,
                            LoginDeviceType.Desktop)))
                .GetResult()
        );

        app.MapPost("/User/Logout",
            async (
                    [FromServices] IUserController controller,
                    [FromServices] IHttpContextAccessor httpContextAccessor) =>
                (await ResultContainer
                    .Start()
                    .Authorize(httpContextAccessor.HttpContext)
                    .ProcessAsync(
                        async () =>
                            await controller.LogOutUser(httpContextAccessor.HttpContext)))
                .GetResult()
        );

        #endregion

        #region UserSettings

        app.MapGet("/Desktop/UserSettings/Get/{userID}",
            async (
                string userId,
                [FromServices] IUserSettingsController controller,
                [FromServices] IHttpContextAccessor httpContextAccessor) =>
            {
                var getRequest = new UserSettingsGetRequest(userId);

                return (await ResultContainer
                        .Start()
                        .Authorize(httpContextAccessor.HttpContext)
                        .Validate<UserSettingsGetRequest, UserSettingsGetRequestValidator>(getRequest)
                        .ProcessAsync(
                            async () =>
                                await controller.GetUserSettings(getRequest)
                        ))
                    .GetResult();
            }
        );

        app.MapPost("/Desktop/UserSettings/Update",
            async (
                    [FromBody] UserSettingsUpdateRequest updateRequest,
                    [FromServices] IUserSettingsController controller,
                    [FromServices] IHttpContextAccessor httpContextAccessor) =>
                (await ResultContainer
                    .Start()
                    .Authorize(httpContextAccessor.HttpContext)
                    .Validate<UserSettingsUpdateRequest, UserSettingsUpdateRequestValidator>(updateRequest)
                    .ProcessAsync(
                        async () =>
                            await controller.UpdateUserSettings(updateRequest)
                    ))
                .GetResult()
        );

        #endregion

        #region Account

        app.MapPost("/Desktop/Account/Register",
            async (
                    [FromBody] AccountRegisterRequest accountRegisterRequest,
                    [FromServices] IAccountsController controller,
                    [FromServices] IHttpContextAccessor httpContextAccessor) =>
                (await ResultContainer
                    .Start()
                    .Validate<AccountRegisterRequest, DeviceRegisterRequestValidator>(accountRegisterRequest)
                    .Authorize(httpContextAccessor.HttpContext)
                    .ProcessAsync(async () =>
                        await controller.RegisterAccount(accountRegisterRequest)))
                .GetResult()
        );

        app.MapPost("/Desktop/Account/Update", async (
                [FromBody] AccountUpdateRequest accountUpdateRequest,
                [FromServices] IAccountsController controller,
                [FromServices] IHttpContextAccessor httpContextAccessor) =>
            (await ResultContainer
                .Start()
                .Validate<AccountUpdateRequest, DeviceUpdateRequestValidator>(accountUpdateRequest)
                .Authorize(httpContextAccessor.HttpContext)
                .ProcessAsync(async () =>
                    await controller.UpdateAccount(accountUpdateRequest)))
            .GetResult()
        );

        app.MapPost("/Desktop/Account/Delete", async (
                [FromBody] AccountDeleteRequest accountDeleteRequest,
                [FromServices] IAccountsController controller,
                [FromServices] IHttpContextAccessor httpContextAccessor) =>
            (await ResultContainer
                .Start()
                .Validate<AccountDeleteRequest, DeviceDeleteRequestValidator>(accountDeleteRequest)
                .Authorize(httpContextAccessor.HttpContext)
                .ProcessAsync(async () =>
                    await controller.DeleteAccount(accountDeleteRequest)))
            .GetResult()
        );

        app.MapPost("/Desktop/Account/SetBalance", async (
                [FromBody] AccountSetBalanceRequest accountSetBalanceRequest,
                [FromServices] IAccountsController controller,
                [FromServices] IHttpContextAccessor httpContextAccessor) =>
            (await ResultContainer
                .Start()
                .Validate<AccountSetBalanceRequest, AccountSetBalanceRequestValidator>(accountSetBalanceRequest)
                .Authorize(httpContextAccessor.HttpContext)
                .ProcessAsync(async () =>
                    await controller.SetAccountBalance(accountSetBalanceRequest)))
            .GetResult()
        );

        app.MapGet("/Mobile/Account/GetForDevice/{userId}/{accountGroup:int}/{deviceId:int}",
            async (
                string userId,
                int accountGroup,
                int deviceId,
                [FromServices] IAccountsController controller,
                [FromServices] IHttpContextAccessor httpContextAccessor) =>
            {
                // Create the request object from the URL parameters 
                var accountGetRequest = new AccountGetForDeviceRequest(userId, accountGroup, deviceId);

                // Process the request using the same logic
                return (await ResultContainer
                        .Start()
                        .Validate<AccountGetForDeviceRequest, AccountGetForDeviceRequestValidator>(accountGetRequest)
                        .Authorize(httpContextAccessor.HttpContext)
                        .ProcessAsync(async () =>
                            await controller.GetAccountsForDevice(accountGetRequest)))
                    .GetResult();
            });

        app.MapGet("/Desktop/Account/GetAllForUser/{userId}",
            async (
                string userId,
                [FromServices] IAccountsController controller,
                [FromServices] IHttpContextAccessor httpContextAccessor) =>
            {
                // Create the request object from the URL parameter
                var getAllForUserRequest = new AccountGetAllForUserRequest(userId);

                // Process the request using the same logic
                return (await ResultContainer
                        .Start()
                        .Validate<AccountGetAllForUserRequest, AccountGetAllForUserRequestValidator>(
                            getAllForUserRequest)
                        .Authorize(httpContextAccessor.HttpContext)
                        .ProcessAsync(async () => await controller.GetAllAccountsForUser(getAllForUserRequest)))
                    .GetResult();
            });

        app.MapGet("/Desktop/Account/GetAllGroupsForUser/{userId}",
            async (
                string userId,
                [FromServices] IAccountsController controller,
                [FromServices] IHttpContextAccessor httpContextAccessor) =>
            {
                // Create the request object from the URL parameter
                var getAllForUserRequest = new AccountGetAllGroupsForUserRequest(userId);

                // Process the request using the same logic
                return (await ResultContainer
                        .Start()
                        .Validate<AccountGetAllGroupsForUserRequest,AccountGetAllGroupsForUserRequestValidator>(
                            getAllForUserRequest)
                        .Authorize(httpContextAccessor.HttpContext)
                        .ProcessAsync(async () => await controller.GetAllAccountGroupsForUser(getAllForUserRequest)))
                    .GetResult();
            });
        
        app.MapGet("/Desktop/Account/GetAllAccountNumbersForUser/{userId}",
            async (
                string userId,
                [FromServices] IAccountsController controller,
                [FromServices] IHttpContextAccessor httpContextAccessor) =>
            {
                // Create the request object from the URL parameter
                var getAllForUserRequest = new AccountGetAllAccountNumbersForUserRequest(userId);
                
                // Process the request using the same logic
                return (await ResultContainer
                        .Start()
                        .Validate<AccountGetAllAccountNumbersForUserRequest,AccountGetAllAccountNumbersForUserRequestValidator>(
                            getAllForUserRequest)
                        .Authorize(httpContextAccessor.HttpContext)
                        .ProcessAsync(async () => await controller.GetAllAccountNumbersForUser(getAllForUserRequest)))
                    .GetResult();
            });
        #endregion

        #region Transaction

        app.MapPost("/Mobile/Transaction/HandleNew",
            async (
                    [FromBody] NotificationHandleRequest handleNotificationRequest,
                    [FromServices] ITransactionController controller,
                    [FromServices] IHttpContextAccessor httpContextAccessor) =>
                (await ResultContainer
                    .Start()
                    .Validate<NotificationHandleRequest, NotificationHandleRequestValidator>(handleNotificationRequest)
                    .Authorize(httpContextAccessor.HttpContext)
                    .ProcessAsync(async () => await controller.RegisterNewTransaction(httpContextAccessor.HttpContext,handleNotificationRequest)))
                .GetResult()
        );


        app.MapPost("/Desktop/Transaction/GetMessages", async (
                [FromBody] MessagesGetRequest messagesGetRequest,
                [FromServices] ITransactionController controller,
                [FromServices] IHttpContextAccessor httpContextAccessor) =>
            (await ResultContainer
                .Start()
                .Validate<MessagesGetRequest, MessagesGetRequestValidator>(messagesGetRequest)
                .Authorize(httpContextAccessor.HttpContext)
                .ProcessAsync(async () =>
                    await controller.GetMessages(httpContextAccessor.HttpContext,messagesGetRequest)))
            .GetResult()
        );

        app.MapPost("/Desktop/Transaction/CalculateBalance", async (
                [FromBody] CalculateBalanceRequest request,
                [FromServices] ITransactionController controller,
                [FromServices] IHttpContextAccessor httpContextAccessor) =>
            (await ResultContainer
                .Start()
                .Validate<CalculateBalanceRequest, CalculateBalanceRequestValidator>(request)
                // .Authorize(httpContextAccessor.HttpContext)
                .ProcessAsync(async () =>
                    await controller.CalculateBalance(request)))
            .GetResult()
        );

        #endregion

        #endregion


//TODO: create new endpoint filtration and validation
//TODO: try to rebuild project to mvc
    }


    public static async Task<IResult> GetHttpContextLog(HttpContext context)
    {
        // Log request details
        var request = context.Request;
        var headers = request.Headers;
        var queryParams = request.Query;
        var method = request.Method;
        var path = request.Path;
        var connectionId = context.Connection.Id;
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
            ConnectionId = connectionId,
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