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
            Users =context.Users.ConvertToDtoList(),
            UserSettings = context.UserSettings.ConvertToDtoList(),
            Accounts = context.Accounts.ConvertToDtoList(),
            Transactions =context.Transactions.ConvertToDtoList()
        }));

        app.MapPost("/Check",
            ([FromServices] IRegisterUser reg, [FromServices] IGetUser get) =>
                TypedResults.Ok(object.ReferenceEquals(reg, get)));

        #endregion

        #region User

        app.MapPost("/Desktop/User/Register",
            async (
                    [FromBody] UserRegisterRequest userRegisterRequest,
                    [FromServices] IUserController controller,
                    [FromServices] ICheckEmailAlreadyRegistered checkEmailRegistered,
                    [FromServices] ICheckUserWithUsernameExist checkUserRegistered,
                    [FromServices] IRegisterUser registerUser,
                    [FromServices] ICreateUserSettings createUserSettings) =>
                (await ResultContainer
                    .Start()
                    .Validate<UserRegisterRequest, UserRegisterRequestValidator>(userRegisterRequest)
                    .ProcessAsync(
                        async () =>
                            await controller.RegisterNewUser(userRegisterRequest, checkEmailRegistered,
                                checkUserRegistered, registerUser, createUserSettings)))
                .GetResult());

        app.MapPost("/Mobile/User/Login",
            async (
                    [FromBody] UserLoginRequest userLoginRequest,
                    [FromServices] IUserController controller,
                    [FromServices] IGetUser getUser,
                    [FromServices] IHttpContextAccessor httpContextAccessor) =>
                (await ResultContainer
                    .Start()
                    .Validate<UserLoginRequest, UserLoginRequestValidator>(userLoginRequest)
                    .ProcessAsync(
                        async () => await controller.LogInUser(userLoginRequest, httpContextAccessor.HttpContext,
                            LoginDeviceType.Mobile,
                            getUser)))
                .GetResult()
        );

        app.MapPost("/Desktop/User/Login",
            async (
                    [FromBody] UserLoginRequest userLoginRequest,
                    [FromServices] IUserController controller,
                    [FromServices] IGetUser getUser,
                    [FromServices] IHttpContextAccessor httpContextAccessor) =>
                (await ResultContainer
                    .Start()
                    .Validate<UserLoginRequest, UserLoginRequestValidator>(userLoginRequest)
                    .ProcessAsync(
                        async () => await controller.LogInUser(userLoginRequest, httpContextAccessor.HttpContext,
                            LoginDeviceType.Desktop,
                            getUser)))
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
                [FromServices] IGetUserSettingsByUserId getUserSettings,
                [FromServices] IHttpContextAccessor httpContextAccessor) =>
            {
                var getRequest = new UserSettingsGetRequest(userId);

                return (await ResultContainer
                        .Start()
                        .Authorize(httpContextAccessor.HttpContext)
                        .Validate<UserSettingsGetRequest, UserSettingsGetRequestValidator>(getRequest)
                        .ProcessAsync(
                            async () =>
                                await controller.GetUserSettings(getRequest, getUserSettings)
                        ))
                    .GetResult();
            }
        );

        app.MapPost("/Desktop/UserSettings/Update",
            async (
                    [FromBody] UserSettingsUpdateRequest updateRequest,
                    [FromServices] IUserSettingsController controller,
                    [FromServices] IGetUserSettingsByUserId getUserSettings,
                    [FromServices] IUpdateUserSettings updateUserSettings,
                    [FromServices] IHttpContextAccessor httpContextAccessor) =>
                (await ResultContainer
                    .Start()
                    .Authorize(httpContextAccessor.HttpContext)
                    .Validate<UserSettingsUpdateRequest, UserSettingsUpdateRequestValidator>(updateRequest)
                    .ProcessAsync(
                        async () =>
                            await controller.UpdateUserSettings(updateRequest, getUserSettings, updateUserSettings)
                    ))
                .GetResult()
        );

        #endregion

        #region Account

        app.MapPost("/Desktop/Account/Register",
            async (
                    [FromBody] AccountRegisterRequest accountRegisterRequest,
                    [FromServices] IAccountsController controller,
                    [FromServices] ICanProceedRequest proceedRequest,
                    [FromServices] IRegisterAccount registerAccount,
                    [FromServices] ICheckUserWithIdExist checkExist,
                    [FromServices] IHttpContextAccessor httpContextAccessor) =>
                (await ResultContainer
                    .Start()
                    .Validate<AccountRegisterRequest, DeviceRegisterRequestValidator>(accountRegisterRequest)
                    .Authorize(httpContextAccessor.HttpContext)
                    .ProcessAsync(async () =>
                        await controller.RegisterAccount(accountRegisterRequest, proceedRequest, registerAccount,
                            checkExist)))
                .GetResult()
        );

        app.MapPost("/Desktop/Account/Update", async (
                [FromBody] AccountUpdateRequest accountUpdateRequest,
                [FromServices] IAccountsController controller,
                [FromServices] IFindAccountByAccountId findAccount,
                [FromServices] ICanProceedRequest proceedRequest,
                [FromServices] IUpdateAccount updateAccount,
                [FromServices] IHttpContextAccessor httpContextAccessor) =>
            (await ResultContainer
                .Start()
                .Validate<AccountUpdateRequest, DeviceUpdateRequestValidator>(accountUpdateRequest)
                .Authorize(httpContextAccessor.HttpContext)
                .ProcessAsync(async () =>
                    await controller.UpdateAccount(accountUpdateRequest, findAccount, proceedRequest, updateAccount)))
            .GetResult()
        );

        app.MapPost("/Desktop/Account/Delete", async (
                [FromBody] AccountDeleteRequest accountDeleteRequest,
                [FromServices] IAccountsController controller,
                [FromServices] IFindAccountByAccountId findAccount,
                [FromServices] IDeleteAccount deleteAccount,
                [FromServices] IHttpContextAccessor httpContextAccessor) =>
            (await ResultContainer
                .Start()
                .Validate<AccountDeleteRequest, DeviceDeleteRequestValidator>(accountDeleteRequest)
                .Authorize(httpContextAccessor.HttpContext)
                .ProcessAsync(async () =>
                    await controller.DeleteAccount(accountDeleteRequest, findAccount, deleteAccount)))
            .GetResult()
        );

        app.MapGet("/Mobile/Account/GetForDevice/{userId}/{accountGroup:int}/{deviceId:int}",
            async (
                string userId,
                int accountGroup,
                int deviceId,
                [FromServices] IAccountsController controller,
                [FromServices] ICheckUserWithIdExist checkUser,
                [FromServices] IFindAccountsByUserId findAccounts,
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
                            await controller.GetAccountsForDevice(accountGetRequest, checkUser, findAccounts)))
                    .GetResult();
            });

        app.MapGet("/Desktop/Account/GetAllForUser/{userId}",
            async (
                string userId,
                [FromServices] IAccountsController controller,
                [FromServices] ICheckUserWithIdExist idExist,
                [FromServices] IFindAccountsByUserId findAccountsByUserId,
                [FromServices] IHttpContextAccessor httpContextAccessor) =>
            {
                // Create the request object from the URL parameter
                var getAllForUserRequest = new AccountGetAllForUserRequest(userId);

                // Process the request using the same logic
                return (await ResultContainer
                        .Start()
                        .Validate<AccountGetAllForUserRequest, AccountGetAllForUserRequestValidator>(getAllForUserRequest)
                        .Authorize(httpContextAccessor.HttpContext)
                        .ProcessAsync(async () => await controller.GetAllAccountsForUser(getAllForUserRequest, idExist,
                            findAccountsByUserId)))
                    .GetResult();
            });

        #endregion

        #region Transaction

        app.MapPost("/Mobile/Transaction/HandleNew",
            async (
                NotificationHandleRequest handleNotificationRequest,
                ITransactionController controller,
                INotificationMessageParser messageParser, IGetAccountByUserIdAndBankCardNumber getUser,
                IRegisterTransaction transactionRegister, IMessageSender sender,
                IGetTransactionsForAccount getTransactions,IGetAccountsForUser getAccounts,
                IGetUserSettingsByUserId  getUserSettings,
                    [FromServices] IHttpContextAccessor httpContextAccessor) =>
                (await ResultContainer
                    .Start()
                    .Validate<NotificationHandleRequest, NotificationHandleRequestValidator>(handleNotificationRequest)
                    .Authorize(httpContextAccessor.HttpContext)
                    .ProcessAsync(async () => await controller.RegisterNewTransaction(handleNotificationRequest,
                        messageParser, getUser, transactionRegister, sender,getTransactions ,getAccounts,getUserSettings)))
                .GetResult()
        );


        app.MapPost("/Desktop/Transaction/GetMessages", async (
                [FromBody] MessagesGetRequest messagesGetRequest,
                [FromServices] ITransactionController controller,
                [FromServices] IGetMessages getMessages,
                [FromServices] IFindAccountByAccountId findAccount, 
                [FromServices] IGetAccountByUserIdAndAccountNumber getAccount,
                [FromServices] IFindAccountsByUserId findAccounts,
                [FromServices] IHttpContextAccessor httpContextAccessor) =>
            (await ResultContainer
                .Start()
                .Validate<MessagesGetRequest, MessagesGetRequestValidator>(messagesGetRequest)
                .Authorize(httpContextAccessor.HttpContext)
                .ProcessAsync(async () =>
                    await controller.GetMessages(messagesGetRequest, getMessages, findAccount, getAccount, findAccounts)))
            .GetResult()
        );

        app.MapPost("/Desktop/Transaction/CalculateBalance", async (
                [FromBody] CalculateBalanceRequest request,
                [FromServices] ITransactionController controller,
                [FromServices] IGetTransactionsForAccount getTransactions,
                [FromServices] IGetAccountsForUser getAccounts,
                [FromServices] IGetUserSettingsByUserId getUserSettings,
                [FromServices] IHttpContextAccessor httpContextAccessor) =>
            (await ResultContainer
                .Start()
                .Validate<CalculateBalanceRequest, CalculateBalanceRequestValidator>(request)
                .Authorize(httpContextAccessor.HttpContext)
                .ProcessAsync(async () =>
                    await controller.CalculateBalance(request, getTransactions, getAccounts, getUserSettings)))
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