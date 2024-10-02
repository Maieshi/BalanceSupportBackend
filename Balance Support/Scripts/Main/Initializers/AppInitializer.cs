using System.Security.Claims;
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
        app.MapGet("/GetContext", (HttpContext context) => TypedResults.Ok(new UserAuthDto(context.User)));
        app.MapPost("/PostContext", (HttpContext context) => TypedResults.Ok(new UserAuthDto(context.User)));

        app.MapPost("/todos", (ToDo todo) =>
        {
            todos.Add(todo);
            return TypedResults.Created($"/todos/{todo.id}", todo);
        });
        app.MapGet("/GetDatabase", async (ApplicationDbContext context) => TypedResults.Ok(new { 
            Users = UserDto.CreateDtos(await context.Users.ToListAsync()), 
            UserSettings = UserSettingsDto.CreateDtos(await context.UserSettings.ToListAsync()), 
            Accounts = AccountDto.CreateDtos(await context.Accounts.ToListAsync()), 
            Transactions = TransactionDto.CreateDtos(await context.Transactions.ToListAsync()), 
            UserTokens =UserTokenDto.CreateDtos(await context.UserTokens.ToListAsync())
        }));
        #endregion

        #region User

        app.MapPost("/Desktop/User/Register",
            async ([FromBody] UserRegisterRequest userRegisterRequest, IAuthUserProvider authProvider) =>
            ResultContainer
                .Start()
                .Validate<UserRegisterRequest, UserRegisterRequestValidator>(userRegisterRequest)
                .Process(
                    async () =>
                        await authProvider.RegisterNewUser(userRegisterRequest.DisplayName, userRegisterRequest.Email,
                            userRegisterRequest.Password))
                .GetResult());

        app.MapPost("/Mobile/User/Login",
            ([FromBody] UserLoginRequest userSignData, IAuthUserProvider authProvider, HttpContext context) =>
                ResultContainer
                    .Start()
                    .Validate<UserLoginRequest, UserLoginRequestValidator>(userSignData)
                    .Process(
                        async () =>
                            await authProvider.LogInUser(context,
                                userSignData.UserCred,
                                userSignData.Password,
                                LoginDeviceType.Mobile))
                    .GetResult()
        );

        app.MapPost("/Desktop/User/Login",
            async ([FromBody] UserLoginRequest userSignData, IAuthUserProvider authProvider, HttpContext context) =>
            ResultContainer
                .Start()
                .Validate<UserLoginRequest, UserLoginRequestValidator>(userSignData)
                .Process(
                    async () =>
                        await authProvider.LogInUser(context,
                            userSignData.UserCred,
                            userSignData.Password,
                            LoginDeviceType.Desktop))
                .GetResult()
        );

        app.MapPost("/User/Logout",
            async (IAuthUserProvider authProvider, HttpContext context) =>
                ResultContainer
                    .Start()
                    .Authorize(context)
                    .Process(
                        async () =>
                            await authProvider.LogOutUser())
                    .GetResult()
        );

        #endregion

        #region UserSettings

        app.MapGet("/Desktop/UserSettings/Get/{userID}",
            async (string userId, IDatabaseUserSettingsProvider provider,
                HttpContext context) =>
            {
                var getRequest = new UserSettingsGetRequest(userId);
                
                return ResultContainer
                    .Start()
                    .Authorize(context)
                    .Validate<UserSettingsGetRequest, UserSettingsGetRequestValidator>(getRequest)
                    .Process(
                        async () =>
                            await provider.GetUserSettings(getRequest)
                    )
                    .GetResult();
            }
        );

        app.MapPost("/Desktop/UserSettings/Update",
            async ([FromBody] UserSettingsUpdateRequest updateRequest, IDatabaseUserSettingsProvider provider,
                    HttpContext context) =>
                ResultContainer
                    .Start()
                    .Authorize(context)
                    .Validate<UserSettingsUpdateRequest, UserSettingsUpdateRequestValidator>(updateRequest)
                    .Process(
                        async () =>
                            await provider.UpdateUserSettings(updateRequest)
                    )
                    .GetResult()
        );

        #endregion

        #region Account

        app.MapPost("/Desktop/Account/Register", async ([FromBody] AccountRegisterRequest deviceRegisterData,
                IDatabaseAccountProvider deviceProvider, HttpContext context) =>
            ResultContainer
                .Start()
                .Validate<AccountRegisterRequest, DeviceRegisterRequestValidator>(deviceRegisterData)
                .Authorize(context)
                .Process(async () => await deviceProvider.RegisterAccount(deviceRegisterData))
                .GetResult()
        );

        app.MapPost("/Desktop/Account/Update", async ([FromBody] AccountUpdateRequest deviceRegisterData,
                IDatabaseAccountProvider deviceProvider, HttpContext context) =>
            ResultContainer
                .Start()
                .Validate<AccountUpdateRequest, DeviceUpdateRequestValidator>(deviceRegisterData)
                .Authorize(context)
                .Process(async () => await deviceProvider.UpdateAccount(deviceRegisterData))
                .GetResult()
        );

        app.MapPost("/Desktop/Account/Delete", async ([FromBody] AccountDeleteRequest deviceRegisterData,
                IDatabaseAccountProvider deviceProvider, HttpContext context) =>
            ResultContainer
                .Start()
                .Validate<AccountDeleteRequest, DeviceDeleteRequestValidator>(deviceRegisterData)
                .Authorize(context)
                .Process(async () => await deviceProvider.DeleteAccount(deviceRegisterData))
                .GetResult()
        );

        app.MapGet("/Mobile/Account/GetForDevice/{userId}/{accountGroup:int}/{deviceId:int}",
            async (string userId, int accountGroup, int deviceId,
                IDatabaseAccountProvider deviceProvider, HttpContext context) =>
            {
                // Create the request object from the URL parameters 
                var deviceGetRequestData = new AccountGetForDeviceRequest(userId, accountGroup, deviceId);

                // Process the request using the same logic
                return ResultContainer
                    .Start()
                    .Validate<AccountGetForDeviceRequest, AccountGetForDeviceRequestValidator>(deviceGetRequestData)
                    .Authorize(context)
                    .Process(async () => await deviceProvider.GetAccountsForDevice(deviceGetRequestData))
                    .GetResult();
            });

        app.MapGet("/Desktop/Account/GetAllForUser/{userId}",
            async (string userId, IDatabaseAccountProvider deviceProvider, HttpContext context) =>
            {
                // Create the request object from the URL parameter
                var getAllForUserRequest = new AccountGetAllForUserRequest(userId);

                // Process the request using the same logic
                return ResultContainer
                    .Start()
                    .Validate<AccountGetAllForUserRequest, AccountGetAllForUserRequestValidator>(getAllForUserRequest)
                    .Authorize(context)
                    .Process(async () => await deviceProvider.GetAllAccountsForUser(getAllForUserRequest))
                    .GetResult();
            });

        #endregion

        #region UserToken

        app.MapPost("/Desktop/UserToken/Set", async ([FromBody] SetUserTokenRequest userTokenRequest,
                ICloudMessagingProvider cloudMessagingProvider, HttpContext context) =>
            ResultContainer
                .Start()
                .Validate<SetUserTokenRequest, SetUserTokenRequestValidator>(userTokenRequest)
                .Authorize(context)
                .Process(async () => await cloudMessagingProvider.SetUserToken(userTokenRequest))
                .GetResult()
        );

        app.MapPost("/Desktop/UserToken/Delete", async ([FromBody] DeleteUserTokenRequest userTokenRequest,
                ICloudMessagingProvider cloudMessagingProvider, HttpContext context) =>
            ResultContainer
                .Start()
                .Validate<DeleteUserTokenRequest, UserTokenDeleteRequestValidator>(userTokenRequest)
                .Authorize(context)
                .Process(async () => await cloudMessagingProvider.DeleteUserToken(userTokenRequest))
                .GetResult()
        );
        #endregion
        
        #region Transaction

        app.MapPost("/Mobile/Transaction/HandleNew", async (
                [FromBody] NotificationHandleRequest handleNotificationRequest,
                INotificationHandler notificationHandler, HttpContext context) =>
            ResultContainer
                .Start()
                .Validate<NotificationHandleRequest, NotificationHandleRequestValidator>(handleNotificationRequest)
                .Authorize(context)
                .Process(async () => await notificationHandler.HandleNotification(handleNotificationRequest))
                .GetResult()
        );


        app.MapPost("/Desktop/Transaction/GetMessages", async (
                [FromBody] MessagesGetRequest getMessagesRequest,
                IDatabaseTransactionProvider databaseTransactionProvider, HttpContext context) =>
            ResultContainer
                .Start()
                .Validate<MessagesGetRequest, MessagesGetRequestValidator>(getMessagesRequest)
                .Authorize(context)
                .Process(async () => await databaseTransactionProvider.GetMessages(getMessagesRequest))
                .GetResult()
        );

        #endregion
        #endregion
        
//TODO: create new endpoint filtration and validation
//TODO: try to rebuild project to mvc
//TODO: split notification handling to parser(mb static) that returns transaction and transaction handler
//TODO: try to upgrade architecture and make the code less cohesive

    }
}

public record ToDo(int id, string name, bool isComplited);

public class UserAuthDto
{
    public UserAuthDto(ClaimsPrincipal claimsPrincipal)
    {
        IsAuthenticated = claimsPrincipal.Identity?.IsAuthenticated ?? false;
        Name = claimsPrincipal.Identity?.Name;
        Claims = MapClaims(claimsPrincipal.Claims);

        // Assuming you want to map identities from ClaimsPrincipal
        // ClaimsPrincipal does not directly provide access to multiple identities
        // but you can map the main identity
        Identity = new IdentityDto
        {
            Name = claimsPrincipal.Identity?.Name,
            AuthenticationType = claimsPrincipal.Identity?.AuthenticationType,
            IsAuthenticated = claimsPrincipal.Identity?.IsAuthenticated ?? false
        };

        // Identities in ClaimsPrincipal are generally not directly accessible
        // You may need additional logic if you have multiple identities in your use case
        Identities = new List<IdentityDto> { Identity };
    }

    public bool IsAuthenticated { get; set; }
    public string? Name { get; set; }
    public IEnumerable<ClaimDto> Claims { get; set; }
    public IEnumerable<IdentityDto> Identities { get; set; }
    public IdentityDto Identity { get; set; }

    private IEnumerable<ClaimDto> MapClaims(IEnumerable<Claim> claims)
    {
        foreach (var claim in claims)
            yield return new ClaimDto
            {
                Type = claim.Type,
                Value = claim.Value
            };
    }

    public class ClaimDto
    {
        public string? Type { get; set; }
        public string? Value { get; set; }
    }

    public class IdentityDto
    {
        public string AuthenticationType { get; set; }
        public bool IsAuthenticated { get; set; }
        public string? Name { get; set; }
        public string NameClaimType { get; set; }
        public string RoleClaimType { get; set; }
        public object Actor { get; set; }
        public object BootstrapContext { get; set; }
        public string Label { get; set; }
        public IEnumerable<ClaimDto> Claims { get; set; } = new List<ClaimDto>();
    }
}