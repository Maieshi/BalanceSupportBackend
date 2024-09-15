using Balance_Support;
using Balance_Support.Interfaces;
using Balance_Support.Scripts.Extensions;
using Balance_Support.DataClasses.Records.AccountData;
using Balance_Support.DataClasses.Records.NotificationData;
using Balance_Support.DataClasses.Records.UserData;
using Balance_Support.DataClasses.Validators;
using Balance_Support.Scripts.Validators;
using Microsoft.AspNetCore.Mvc;
using Balance_Support.DataClasses.Records;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);


ServicesInitializer.Initialize(builder.Services);

var app = builder.Build();
app.UseCors("AllowClientDomain");
app.UseHttpsRedirection();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();
//app.MapControllers();
var todos = new List<ToDo>();

app.MapGet("/", () => "Hello World!");
app.MapGet("/GetContext", (HttpContext context) => TypedResults.Ok(new UserDto(context.User)));
app.MapPost("/PostContext", (HttpContext context) => TypedResults.Ok(new UserDto(context.User)));
app.MapGet("/todos/{id}", Results<Ok<ToDo>, NotFound> (int id) =>
{
    var targetTodo = todos.FirstOrDefault(x => x.id == id);
    return targetTodo is null
        ? TypedResults.NotFound()
        : TypedResults.Ok(targetTodo);
});
app.MapPost("/todos", (ToDo todo) =>
{
    todos.Add(todo);
    return TypedResults.Created($"/todos/{todo.id}", todo);
    ;
});
app.MapPost("/todosClass", (ToDoClass todo) =>
{
    var rec = new ToDo(todo.Id, todo.name, todo.isComplited);
    todos.Add(rec);
    return TypedResults.Created($"/todos/{rec.id}", rec);
});


app.MapGet("/testEmpty", () => { return "Success"; });
app.MapPost("/testRoute/{id}/{name}", (int id, string name) => { return $"Success {new TestModel(id, name)}"; });
app.MapPost("/testModel", (TestModel model) => { return "Success"; });
app.MapPost("/testModelFromBody", ([FromBody] TestModel model) => { return "Success"; });
app.MapPost("/testFromQuery", (int id, string name) =>
{
    var model = new TestModel(id, name);
    return $"Success {model}";
});
app.MapPost("/testAcceptsModel", (TestModel model) => { return $"Success {model}"; })
    .Accepts<TestModel>("application/json");
app.MapPost("/testAcceptsModelFromBody", ([FromBody] TestModel model) => { return $"Success {model}"; })
    .Accepts<TestModel>("application/json");

#region UserManagement

app.MapPost("/Desktop/User/Register",
    async ([FromBody] UserRegistrationData registration, IAuthUserProvider authProvider) =>
    ResultContainer
        .Start()
        .Validate<UserRegistrationData, UserRegistrationDataValidator>(registration)
        .Process(
            async () =>
                await authProvider.RegisterNewUser(registration.DisplayName, registration.Email,
                    registration.Password))
        .GetResult());

app.MapPost("/Mobile/User/Login",
    ([FromBody] UserLoginData userSignData, IAuthUserProvider authProvider, HttpContext context) =>
        ResultContainer
            .Start()
            .Validate<UserLoginData, UserLoginDataValidator>(userSignData)
            .Process(
                async () =>
                    await authProvider.LogInUser(context,
                        userSignData.UserCred,
                        userSignData.Password,
                        LoginDeviceType.Mobile))
            .GetResult()
);

app.MapPost("/Desktop/User/Login",
    async ([FromBody] UserLoginData userSignData, IAuthUserProvider authProvider, HttpContext context) =>
    ResultContainer
        .Start()
        .Validate<UserLoginData, UserLoginDataValidator>(userSignData)
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

#region AccountManagement

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
        .Process(async () => await deviceProvider.DeleteDevice(deviceRegisterData))
        .GetResult()
);

app.MapGet("/Mobile/Account/GetForDevice/{userId}/{accountGroup:int}/{deviceId:int}",
    async (string userId, int accountGroup, int deviceId,
        IDatabaseAccountProvider deviceProvider, HttpContext context) =>
    {
        // Create the request object from the URL parameters
        var deviceGetRequestData = new AccountGetForDeviceRequest(userId, accountGroup, deviceId);

        // Process the request using the same logic
        return  ResultContainer
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
        return  ResultContainer
            .Start()
            .Validate<AccountGetAllForUserRequest, AccountGetAllForUserRequestValidator>(getAllForUserRequest)
            .Authorize(context)
            .Process(async () => await deviceProvider.GetAllAccountsForUser(getAllForUserRequest))
            .GetResult();
    });

#endregion

#region NotificationManagement

app.MapPost("/Desktop/UserToken/Register", async ([FromBody] UserTokenRequest userTokenRequest,
        ICloudMessagingProvider cloudMessagingProvider, HttpContext context) =>
    ResultContainer
        .Start()
        .Validate<UserTokenRequest, UserTokenRequestValidator>(userTokenRequest)
        .Authorize(context)
        .Process(async () => await cloudMessagingProvider.RegisterUserToken(userTokenRequest))
        .GetResult()
);

app.MapPost("/Desktop/Transaction/Get", async ([FromBody] GetTransactionRequest getTransactionRequest,
        IDatabaseTransactionProvider transactionProvider, HttpContext context) =>
    ResultContainer
        .Start()
        .Validate<GetTransactionRequest, GetTransactionRequestValidatior>(getTransactionRequest)
        .Authorize(context)
        .Process(async () =>
            await transactionProvider.GetTransactionsForUser(getTransactionRequest.UserId,
                getTransactionRequest.Amount))
        .GetResult()
);

app.MapPost("/Desktop/UserToken/Update", async ([FromBody] UserTokenRequest userTokenRequest,
        ICloudMessagingProvider cloudMessagingProvider, HttpContext context) =>
    ResultContainer
        .Start()
        .Validate<UserTokenRequest, UserTokenRequestValidator>(userTokenRequest)
        .Authorize(context)
        .Process(async () => await cloudMessagingProvider.UpdateUserToken(userTokenRequest))
        .GetResult()
);

app.MapPost("/Mobile/Notification/Handle", async ([FromBody] NotificationHandleRequest handleNotificationRequest,
        INotificationHandler notificationHandler, HttpContext context) =>
    ResultContainer
        .Start()
        .Validate<NotificationHandleRequest, NotificationHandleRequestValidator>(handleNotificationRequest)
        .Authorize(context)
        .Process(async () => await notificationHandler.HandleNotification(handleNotificationRequest))
        .GetResult()
);

#endregion

app.Run();

public class ToDoClass
{
    public int Id { get; set; }

    public string name { get; set; }

    public bool isComplited { get; set; }
}

public record ToDo(int id, string name, bool isComplited);

public class UserDto
{
    public bool IsAuthenticated { get; set; }
    public string? Name { get; set; }
    public IEnumerable<ClaimDto> Claims { get; set; }
    public IEnumerable<IdentityDto> Identities { get; set; }
    public IdentityDto Identity { get; set; }

    public UserDto(ClaimsPrincipal claimsPrincipal)
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

    private IEnumerable<ClaimDto> MapClaims(IEnumerable<Claim> claims)
    {
        foreach (var claim in claims)
        {
            yield return new ClaimDto
            {
                Type = claim.Type,
                Value = claim.Value
            };
        }
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