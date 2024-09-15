using System.Diagnostics;
using Balance_Support.DataClasses.Validators;
using Balance_Support.Interfaces;
using Balance_Support.SerializationClasses;
using Newtonsoft.Json;
using Firebase.Auth;
using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Firebase.Database;
using Firebase.Database.Query;
using Google.Apis.Auth.OAuth2;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

namespace Balance_Support;

public static class ServicesInitializer
{
    public static void Initialize(WebApplicationBuilder builder)
    {
        var services = builder.Services;
        
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("SqlServerConnection"))); // Register EF Core DbContext with SQL Server
        
        FirebaseAuthApiKey apiKey = JsonConvert.DeserializeObject<FirebaseAuthApiKey>(
            File.ReadAllText(Path.Combine(PathStorage.FirebaseConfigsPath, PathStorage.FirebaseAuthApiKey)));

        FirebaseDatabaseClientConfig databaseConfig = JsonConvert.DeserializeObject<FirebaseDatabaseClientConfig>(
            File.ReadAllText(
                Path.Combine(PathStorage.FirebaseConfigsPath, PathStorage.FirebaseDatabaseClientConfigJson)));

        FirebaseApp.Create(new AppOptions
        {
            Credential = GoogleCredential.FromFile(Path.Combine(PathStorage.FirebaseConfigsPath, PathStorage.FirebaseCloudMessagingJson)),
        });

     
        services.AddHttpContextAccessor();
        //services.AddFluentValidation();
        services.AddAuthorization();
    

        services.AddDistributedMemoryCache();

        services.AddSession(options =>
        {
            options.IdleTimeout = TimeSpan.FromHours(24);
            options.Cookie.HttpOnly = true;
            options.Cookie.IsEssential = true;
        });

        services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
        .AddCookie(options =>
        {
            // Set up cookie options
            options.Cookie.HttpOnly = true;
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // Ensure cookie is sent only over HTTPS
            options.Cookie.SameSite = SameSiteMode.None; // Allow cookies in cross-site requests
            options.Cookie.Name = "AuthCookie"; // Name your authentication cookie
            options.Cookie.Domain = ".balancesupportapi.top"; // Specify the domain for the cookie
            options.Cookie.Path = "/"; // Path for the cookie
            options.LoginPath = "/account/login"; // Redirect to login if unauthorized
            options.LogoutPath = "/account/logout"; // Path to handle logout
            options.AccessDeniedPath = "/account/accessdenied"; // Path for access denied page
            options.SlidingExpiration = true; // Automatically extend the cookie lifetime when the user is active
            options.ExpireTimeSpan = TimeSpan.FromDays(7); // Set cookie expiration
        });

        services.AddCors(options =>
        {
            options.AddPolicy("AllowClientDomain", builder =>
            {
                builder.WithOrigins("http://localhost:5173", "https://balance-support.vercel.app") // Client domain
                       .AllowAnyHeader()
                       .AllowAnyMethod()
                       .AllowCredentials(); // Allow cookies to be sent in requests
            });
        });

        services.AddAuthorization();
        //services.AddFluentValidation(fv => fv.RegisterValidatorsFromAssemblyContaining<DeviceUpdateRequestValidator>());

        services.AddSingleton<FirebaseClient>(
            new FirebaseClient("https://balance-support-b9da3-default-rtdb.europe-west1.firebasedatabase.app/",
                new FirebaseOptions { AuthTokenAsyncFactory = () => GetTokenByGoogleServices(), AsAccessToken = true })
        );

        services.AddSingleton<IFirebaseAuthProvider>(
            new FirebaseAuthProvider(new Firebase.Auth.FirebaseConfig(apiKey.ApiKey))
        );

        services.AddScoped<IDatabaseUserProvider, DatabaseUserProvider>();
        services.AddScoped<IAuthUserProvider, AuthUserProvider>();
        services.AddScoped<IDatabaseAccountProvider, DatabaseAccountProvider>();
        services.AddScoped<ICloudMessagingProvider, CloudMessagingProvider>();
        services.AddScoped<IDatabaseTransactionProvider, DatabaseTransactionProvider>();
        services.AddScoped<INotificationHandler, NotificationHandler>();
        services.AddScoped<FirebaseToSqlServerMigrator>();

        // Add Authentication services
        //services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
        //    .AddCookie(options =>
        //    {
        //        options.LoginPath = "/Account/Login"; // Adjust paths as necessary
        //        options.LogoutPath = "/Account/Logout";
        //        options.AccessDeniedPath = "/Account/AccessDenied";
        //    });

        //services.AddAuthorization();

        // Example of building service provider (not recommended in most cases for normal DI usage)
        var provider = services.BuildServiceProvider();
        provider.GetService<FirebaseToSqlServerMigrator>().Migrate();
        //provider.GetService<INotificationHandler>().Test();
    } 

    private static async Task<string> GetTokenByGoogleServices()
    {
        var credential = GoogleCredential.FromFile(Path.Combine(PathStorage.FirebaseConfigsPath, PathStorage.FirebaseCloudMessagingJson)).CreateScoped(new string[]
        {
            "https://www.googleapis.com/auth/userinfo.email",
            "https://www.googleapis.com/auth/firebase.database"
        });

        ITokenAccess c = credential as ITokenAccess;
        return await c.GetAccessTokenForRequestAsync();
    }
}
