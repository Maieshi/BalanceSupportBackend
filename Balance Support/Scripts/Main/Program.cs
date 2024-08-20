using System.Diagnostics;
using Balance_Support;
using FireSharp;
using FireSharp.Config;
using FireSharp.Interfaces;
using Balance_Support.Interfaces;
using System.ComponentModel.DataAnnotations;
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
app.MapPost("/Register",
    async (UserReginstrationData registration, IAuthUserProvider authProvider) =>
        await authProvider.RegisterNewUser(registration.DisplayName, registration.Email, registration.Password));

app.MapPost("/Mobile/Login",
    async (UserSignData userSignData, IAuthUserProvider authProvider, HttpContext context) =>
        await authProvider.LogInUser(userSignData.UserRecord, userSignData.UserCred, userSignData.Password,
            LoginDeviceType.Mobile));

app.MapPost("/Desktop/Login",
    async (UserSignData userSignData, IAuthUserProvider authProvider, HttpContext context) =>
        await authProvider.LogInUser(userSignData.UserRecord, userSignData.UserCred, userSignData.Password,
            LoginDeviceType.Desktop));

app.MapPost("/Logout",
    async (IAuthUserProvider authProvider) =>
        await authProvider.LogOutUser());

app.MapPost("/Mobile/RegisterDevice", async (DeviceInfo deviceInfo, DeviceInfoValidator validator) =>
{
    var validationResults = new List<ValidationResult>();
    var validationContext = new ValidationContext(deviceInfo);
    
    bool isValid = Validator.TryValidateObject(deviceInfo, validationContext, validationResults, true);

    if (!isValid)
    {
        // Return a bad request with validation errors
        return Results.BadRequest(validationResults);
    }

    // Process the valid data
    // ...

    return Results.Ok();
});

app.MapPost("/Logout",
    async (IAuthUserProvider authProvider) =>
        await authProvider.LogOutUser());

app.Run();


public record ToDo(int id, string name, DateTime dueDate, bool isComplited);

public record UserReginstrationData(string Email, string DisplayName, string Password);

public record UserSignData(string UserRecord, string UserCred, string Password);

public record SimcardData(
   
    int SimId,
   
    string SimNumber,
   
    string BankType,
   
    int CardNumber,
   
    decimal InitalBalance
);

public record DeviceInfo(
    
    string UserRecordId,
   
    string DeviceId,
   
    string LastName,
    
    int DeviceGroup,
    
    int DeviceSubgroup,
    
    List<SimcardData> SimcardsData,
    
    string Description
);