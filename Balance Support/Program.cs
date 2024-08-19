using System.Diagnostics;
using Balance_Support;
using FireSharp;
using FireSharp.Config;
using FireSharp.Interfaces;

var builder = WebApplication.CreateBuilder(args);

DIServiceInitializer.Initialize(builder.Services);

var app = builder.Build();
var todos = new List<ToDo>();



const string DatabaseUrl = "https://balance-support-b9da3-default-rtdb.europe-west1.firebasedatabase.app/";
const string DatabaseSecret = "3J23Se6pRRrvuTiyPKSuLRbIB94GM4jtTqmuf6fe";

IFirebaseClient client = new FirebaseClient(new FireSharp.Config.FirebaseConfig()
{
    AuthSecret = DatabaseSecret,
    BasePath = DatabaseUrl
});

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
    async (UserReginstrationData registration) =>
        await authProvider.RegisterNewUser(registration.DisplayName, registration.Email, registration.Password));

app.MapPost("/Mobile/Login",
    async (UserSignData userSignData) =>
        await authProvider.LogInUser(userSignData.UserRecord, userSignData.UserCred, userSignData.Password,
            LoginDeviceType.Mobile));

app.MapPost("/Desktop/Login",
    async (UserSignData userSignData) =>
        await authProvider.LogInUser(userSignData.UserRecord, userSignData.UserCred, userSignData.Password,
            LoginDeviceType.Desktop));

app.Run();


public record ToDo(int id, string name, DateTime dueDate, bool isComplited);

public record UserReginstrationData(string Email, string DisplayName, string Password);

public record UserSignData(string UserRecord, string UserCred, string Password);