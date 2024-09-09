using System.Diagnostics;
using Balance_Support;
using Balance_Support.Interfaces;
using Balance_Support.Scripts.Extensions;
using System.ComponentModel.DataAnnotations;
using Balance_Support.DataClasses.Records.AccountData;
using Balance_Support.DataClasses.Records.NotificationData;
using Balance_Support.DataClasses.Records.UserData;
using Balance_Support.DataClasses.Validators;
using Balance_Support.Scripts.Validators;
using Microsoft.AspNetCore.Mvc;
using Balance_Support.DataClasses.Records;
using Microsoft.AspNetCore.Http.HttpResults;
using LiteDB;
using Microsoft.EntityFrameworkCore; // Added for EF Core
using Microsoft.EntityFrameworkCore.SqlServer; // Assuming your DbContext is in the Data namespace
using Microsoft.EntityFrameworkCore.Infrastructure; // Added for EF Core
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.


ServicesInitializer.Initialize(builder);

var app = builder.Build();

app.UseCors("AllowLocalhost");
app.UseHttpsRedirection();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

// Test routes
var todos = new List<ToDo>();
app.MapGet("/", () => "Hello World!");
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
    return TypedResults.Created($"/todos/{todo.id}", todo); ;
});

// User management routes
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

// More user, account, and notification management routes here...

app.Run();

// Models
public record ToDo(int id, string name, bool isComplited);

public class ToDoClass
{
    public int Id { get; set; }
    public string name { get; set; }
    public bool isComplited { get; set; }
}
