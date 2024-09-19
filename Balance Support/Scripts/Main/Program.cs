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
using Balance_Support.Scripts.Main.Initializers;

var builder = WebApplication.CreateBuilder(args);

await ServicesInitializer.Initialize(builder);
var app = builder.Build();
await AppInitializer.Initialize(app);
await app.RunAsync();
