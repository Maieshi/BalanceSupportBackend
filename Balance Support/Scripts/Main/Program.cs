using System.Diagnostics;
using Balance_Support;
using FireSharp;
using FireSharp.Config;
using FireSharp.Interfaces;
using Balance_Support.Interfaces;
using Balance_Support.Scripts.Extensions;
using System.ComponentModel.DataAnnotations;
using Balance_Support.Scripts.Extensions;
using Balance_Support.DataClasses.Records.AccountData;
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

// const string DatabaseUrl = "https://balance-support-b9da3-default-rtdb.europe-west1.firebasedatabase.app/";
// const string DatabaseSecret = "3J23Se6pRRrvuTiyPKSuLRbIB94GM4jtTqmuf6fe";
//
// IFirebaseClient client = new FirebaseClient(new FireSharp.Config.FirebaseConfig()
// {
//     AuthSecret = DatabaseSecret,
//     BasePath = DatabaseUrl
// });

// DatabaseUserProvider databaseUserProvider = new(client);
//
// FirebaseAuthUserProvider authProvider = new(databaseUserProvider);
//
// CloudMessagingProvider cloudMessagingProvider = new CloudMessagingProvider();


app.MapGet("/", () => "Hello World!");

app.MapGet("/todos/{id}", (int id) => { return $"todos{id}"; });

app.MapPost("/todos", (ToDo todo) =>
{
    todos.Add(todo);
    return "todos";
});

#region  UserManagement

app.MapPost("/Register",
    async (UserRegistrationData registration, IAuthUserProvider authProvider) =>
        ResultContainer
            .Start()
            .Validate<UserRegistrationData, UserRegistrationDataValidator>(registration)
            .Process(
                async () =>
                    await authProvider.RegisterNewUser(registration.DisplayName, registration.Email,
                        registration.Password))
            .GetResult());

// app.MapPost("/Mobile/Login",
//     async (UserSignData userSignData, IAuthUserProvider authProvider, HttpContext context) =>
//         await authProvider.LogInUser(userSignData.UserRecord, userSignData.UserCred, userSignData.Password,
//             LoginDeviceType.Mobile));

app.MapPost("/Mobile/Login",
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



app.MapPost("/Desktop/Login",
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

app.MapPost("/Logout",
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

#region  DeviceManagement

app.MapPost("/Desktop/Device/Register", async (DeviceRegisterRequest deviceRegisterData,IDatabaseAccountProvider deviceProvider, HttpContext context) =>
    ResultContainer
        .Start()
        .Validate<DeviceRegisterRequest, DeviceRegisterRequestValidator>(deviceRegisterData)
        .Authorize(context)
        .Process(async ()=>await deviceProvider.RegisterAccount(deviceRegisterData))
        .GetResult()
);

app.MapPost("/Desktop/Device/Update", async (DeviceUpdateRequest deviceRegisterData,IDatabaseAccountProvider deviceProvider, HttpContext context) =>
    ResultContainer
        .Start()
        .Validate<DeviceUpdateRequest, DeviceUpdateRequestValidator>(deviceRegisterData)
        .Authorize(context)
        .Process(async ()=>await deviceProvider.UpdateAccount(deviceRegisterData))
        .GetResult()
);

app.MapPost("/Desktop/Device/Delete", async (DeviceDeleteRequest deviceRegisterData,IDatabaseAccountProvider deviceProvider, HttpContext context) =>
    ResultContainer
        .Start()
        .Validate<DeviceDeleteRequest, DeviceDeleteRequestValidator>(deviceRegisterData)
        .Authorize(context)
        .Process(async ()=>await deviceProvider.DeleteDevice(deviceRegisterData))
        .GetResult()
);

app.MapGet("/Mobile/Device/Get", async (DeviceGetRequest deviceGetRequestData,IDatabaseAccountProvider deviceProvider, HttpContext context) =>
    ResultContainer
        .Start()
        .Validate<DeviceGetRequest, DeviceGetRequestvValidator>(deviceGetRequestData)
        .Authorize(context)
        .Process(async ()=>await deviceProvider.GetAccountsByGroupAndDeviceId(deviceGetRequestData))
        .GetResult()
);

#endregion

#region  NotificationManagement



#endregion



app.Run();


public record ToDo(int id, string name, DateTime dueDate, bool isComplited);














