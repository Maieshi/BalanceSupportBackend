using System.Diagnostics;
using Balance_Support;
using Balance_Support.Interfaces;
using Balance_Support.Scripts.Extensions;
using System.ComponentModel.DataAnnotations;
using Balance_Support.Scripts.Extensions;
using Balance_Support.DataClasses.Records.AccountData;
using Balance_Support.DataClasses.Records.NotificationData;
using Balance_Support.DataClasses.Records.UserData;
using Balance_Support.DataClasses.Validators;
using Balance_Support.Scripts.Validators;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(24);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

ServicesInitializer.Initialize(builder.Services);

var app = builder.Build();
app.UseSession();
var todos = new List<ToDo>();

app.MapGet("/", () => "Hello World!");

app.MapGet("/todos/{id}", (int id) => { return $"todos{id}"; });

app.MapPost("/todos", (ToDo todo) =>
{
    todos.Add(todo);
    return "todos";
});

#region  UserManagement

app.MapPost("User/Register",
    async (UserRegistrationData registration, IAuthUserProvider authProvider) =>
        ResultContainer
            .Start()
            .Validate<UserRegistrationData, UserRegistrationDataValidator>(registration)
            .Process(
                async () =>
                    await authProvider.RegisterNewUser(registration.DisplayName, registration.Email,
                        registration.Password))
            .GetResult());

app.MapPost("/Mobile/User/Login",
    (UserLoginData userSignData, IAuthUserProvider authProvider, HttpContext context) =>
        ResultContainer
            .Start()
            .Validate<UserLoginData, UserLoginDataValidator>(userSignData)
            .Authorize(context)
            .Process(
                async () =>
                    await authProvider.LogInUser(
                        userSignData.UserCred,
                        userSignData.Password,
                        LoginDeviceType.Mobile))
            .GetResult()
);

app.MapPost("/Desktop/User/Login",
    async (UserLoginData userSignData, IAuthUserProvider authProvider, HttpContext context) =>
        ResultContainer
            .Start()
            .Validate<UserLoginData, UserLoginDataValidator>(userSignData)
            .Process(
                async () =>
                    await authProvider.LogInUser(
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

#region  AccountManagement

app.MapPost("/Desktop/Account/Register", async (AccountRegisterRequest deviceRegisterData,IDatabaseAccountProvider deviceProvider, HttpContext context) =>
    ResultContainer
        .Start()
        .Validate<AccountRegisterRequest, DeviceRegisterRequestValidator>(deviceRegisterData)
        .Authorize(context)
        .Process(async ()=>await deviceProvider.RegisterAccount(deviceRegisterData))
        .GetResult()
);

app.MapPost("/Desktop/Account/Update", async (AccountUpdateRequest deviceRegisterData,IDatabaseAccountProvider deviceProvider, HttpContext context) =>
    ResultContainer
        .Start()
        .Validate<AccountUpdateRequest, DeviceUpdateRequestValidator>(deviceRegisterData)
        .Authorize(context)
        .Process(async ()=>await deviceProvider.UpdateAccount(deviceRegisterData))
        .GetResult()
);

app.MapPost("/Desktop/Account/Delete", async (AccountDeleteRequest deviceRegisterData,IDatabaseAccountProvider deviceProvider, HttpContext context) =>
    ResultContainer
        .Start()
        .Validate<AccountDeleteRequest, DeviceDeleteRequestValidator>(deviceRegisterData)
        .Authorize(context)
        .Process(async ()=>await deviceProvider.DeleteDevice(deviceRegisterData))
        .GetResult()
);

app.MapGet("/Mobile/Account/Get", async (AccountGetRequest deviceGetRequestData,IDatabaseAccountProvider deviceProvider, HttpContext context) =>
    ResultContainer
        .Start()
        .Validate<AccountGetRequest, DeviceGetRequestvValidator>(deviceGetRequestData)
        .Authorize(context)
        .Process(async ()=>await deviceProvider.GetAccountsForDevice(deviceGetRequestData))
        .GetResult()
);

#endregion

#region  NotificationManagement

app.MapGet("/Desktop/UserToken/Register", async (UserTokenRequest userTokenRequest,ICloudMessagingProvider cloudMessagingProvider, HttpContext context) =>
    ResultContainer
        .Start()
        .Validate<UserTokenRequest, UserTokenRequestValidator>(userTokenRequest)
        .Authorize(context)
        .Process(async ()=>await cloudMessagingProvider.RegisterUserToken(userTokenRequest))
        .GetResult()
);

app.MapGet("/Desktop/UserToken/Update", async (UserTokenRequest userTokenRequest,ICloudMessagingProvider cloudMessagingProvider, HttpContext context) =>
    ResultContainer
        .Start()
        .Validate<UserTokenRequest, UserTokenRequestValidator>(userTokenRequest)
        .Authorize(context)
        .Process(async ()=>await cloudMessagingProvider.UpdateUserToken(userTokenRequest))
        .GetResult()
);

app.MapGet("/Mobile/Notification/Handle", async (NotificationHandleRequest handleNotificationRequest,INotificationHandler notificationHandler, HttpContext context) =>
    ResultContainer
        .Start()
        .Validate<NotificationHandleRequest, NotificationHandleRequestValidator>(handleNotificationRequest)
        .Authorize(context)
        .Process(async ()=>await notificationHandler.HandleNotification(handleNotificationRequest))
        .GetResult()
);

#endregion



app.Run();


public record ToDo(int id, string name, DateTime dueDate, bool isComplited);














