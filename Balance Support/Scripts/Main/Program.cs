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
using Microsoft.AspNetCore.Mvc;
using Balance_Support.DataClasses.Records;
using Microsoft.AspNetCore.Http.HttpResults;
using LiteDB;
using System.Threading.Tasks;

var builder = WebApplication.CreateBuilder(args);

ServicesInitializer.Initialize(builder);

var app = builder.Build();
app.UseCors("AllowLocalhost");
app.UseHttpsRedirection();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();
//app.MapControllers();
var todos = new List<ToDo>();

app.MapGet("/", () => "Hello World!");
app.MapGet("/todos/{id}", Results<Ok<ToDo>, NotFound> (int id) =>
{
    var targetTodo = todos.FirstOrDefault(x=>x.id==id);
    return targetTodo is null
        ? TypedResults.NotFound()
        : TypedResults.Ok(targetTodo);
});
app.MapPost("/todos", (ToDo todo) =>
{
    todos.Add(todo);
    return TypedResults.Created($"/todos/{todo.id}", todo); ;
});
app.MapPost("/todosClass", (ToDoClass todo) =>
{
    var rec = new ToDo(todo.Id, todo.name, todo.isComplited);
    todos.Add(rec);
    return TypedResults.Created($"/todos/{rec.id}", rec);
});


app.MapGet("/testEmpty", () =>
{
    return "Success";
});
app.MapPost("/testRoute/{id}/{name}", (int id, string name) =>
{
    return $"Success {new TestModel(id, name)}";
});
app.MapPost("/testModel", (TestModel model) =>
{
    return "Success";
});
app.MapPost("/testModelFromBody", ([FromBody] TestModel model) =>
{
    return "Success";
});
app.MapPost("/testFromQuery", (int id, string name) =>
{
    var model = new TestModel(id, name);
    return $"Success {model}";
});
app.MapPost("/testAcceptsModel", (TestModel model) =>
{
    return $"Success {model}";
})
.Accepts<TestModel>("application/json");
app.MapPost("/testAcceptsModelFromBody", ([FromBody] TestModel model) =>
{
    return $"Success {model}";
})
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
                    await authProvider.LogInUser(
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

#region AccountManagement

app.MapPost("/Desktop/Account/Register", async ([FromBody] AccountRegisterRequest deviceRegisterData, IDatabaseAccountProvider deviceProvider, HttpContext context) =>
    ResultContainer
        .Start()
        .Validate<AccountRegisterRequest, DeviceRegisterRequestValidator>(deviceRegisterData)
        .Authorize(context)
        .Process(async () => await deviceProvider.RegisterAccount(deviceRegisterData))
        .GetResult()
);

app.MapPost("/Desktop/Account/Update", async ([FromBody] AccountUpdateRequest deviceRegisterData, IDatabaseAccountProvider deviceProvider, HttpContext context) =>
    ResultContainer
        .Start()
        .Validate<AccountUpdateRequest, DeviceUpdateRequestValidator>(deviceRegisterData)
        .Authorize(context)
        .Process(async () => await deviceProvider.UpdateAccount(deviceRegisterData))
        .GetResult()
);

app.MapPost("/Desktop/Account/Delete", async ([FromBody] AccountDeleteRequest deviceRegisterData, IDatabaseAccountProvider deviceProvider, HttpContext context) =>
    ResultContainer
        .Start()
        .Validate<AccountDeleteRequest, DeviceDeleteRequestValidator>(deviceRegisterData)
        .Authorize(context)
        .Process(async () => await deviceProvider.DeleteDevice(deviceRegisterData))
        .GetResult()
);

app.MapGet("/Mobile/Account/Get", async ([FromBody] AccountGetRequest deviceGetRequestData, IDatabaseAccountProvider deviceProvider, HttpContext context) =>
    ResultContainer
        .Start()
        .Validate<AccountGetRequest, DeviceGetRequestvValidator>(deviceGetRequestData)
        .Authorize(context)
        .Process(async () => await deviceProvider.GetAccountsForDevice(deviceGetRequestData))
        .GetResult()
);

#endregion

#region NotificationManagement

app.MapPost("/Desktop/UserToken/Register", async ([FromBody] UserTokenRequest userTokenRequest, ICloudMessagingProvider cloudMessagingProvider, HttpContext context) =>
    ResultContainer
        .Start()
        .Validate<UserTokenRequest, UserTokenRequestValidator>(userTokenRequest)
        .Authorize(context)
        .Process(async () => await cloudMessagingProvider.RegisterUserToken(userTokenRequest))
        .GetResult()
);

app.MapPost("/Desktop/UserToken/Update", async ([FromBody] UserTokenRequest userTokenRequest, ICloudMessagingProvider cloudMessagingProvider, HttpContext context) =>
    ResultContainer
        .Start()
        .Validate<UserTokenRequest, UserTokenRequestValidator>(userTokenRequest)
        .Authorize(context)
        .Process(async () => await cloudMessagingProvider.UpdateUserToken(userTokenRequest))
        .GetResult()
);

app.MapPost("/Mobile/Notification/Handle", async ([FromBody] NotificationHandleRequest handleNotificationRequest, INotificationHandler notificationHandler, HttpContext context) =>
    ResultContainer
        .Start()
        .Validate<NotificationHandleRequest, NotificationHandleRequestValidator>(handleNotificationRequest)
        .Authorize(context)
        .Process(async () => await notificationHandler.HandleNotification(handleNotificationRequest))
        .GetResult()
);

#endregion

app.Run();

public record ToDo(int id, string name, bool isComplited);

public class ToDoClass
{
    public int Id { get; set; }

    public string name { get; set; }

    public bool isComplited { get; set; }
}

