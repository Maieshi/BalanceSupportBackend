using Balance_Support.DataClasses.Records.AccountData;
using Balance_Support.DataClasses.Records.NotificationData;
using Balance_Support.Scripts.Providers;
using Balance_Support.Scripts.Providers.Interfaces;
using Balance_Support.SerializationClasses;
using Firebase.Auth;
using Firebase.Database;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using User = Balance_Support.DataClasses.DatabaseEntities.User;

namespace Balance_Support.Scripts.Main.Initializers;

public static class ServicesInitializer
{
    public static async Task Initialize(WebApplicationBuilder builder)
    {
        var services = builder.Services;

        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(
                builder.Configuration
                    .GetConnectionString("SqlServerConnection"))); // Register EF Core DbContext with SQL Server

        var apiKey = JsonConvert.DeserializeObject<FirebaseAuthApiKey>(
            File.ReadAllText(Path.Combine(PathStorage.FirebaseConfigsPath, PathStorage.FirebaseAuthApiKey)));

        var databaseConfig = JsonConvert.DeserializeObject<FirebaseDatabaseClientConfig>(
            File.ReadAllText(
                Path.Combine(PathStorage.FirebaseConfigsPath, PathStorage.FirebaseDatabaseClientConfigJson)));

        FirebaseApp.Create(new AppOptions
        {
            Credential = GoogleCredential.FromFile(Path.Combine(PathStorage.FirebaseConfigsPath,
                PathStorage.FirebaseCloudMessagingJson))
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
                builder.WithOrigins("http://localhost:5173", "https://balance-support.vercel.app",
                        "https://localhost:7158") // Client domain
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials(); // Allow cookies to be sent in requests
            });
        });

        services.AddAuthorization();
        //services.AddFluentValidation(fv => fv.RegisterValidatorsFromAssemblyContaining<DeviceUpdateRequestValidator>());

        services.AddSingleton(
            new FirebaseClient("https://balance-support-b9da3-default-rtdb.europe-west1.firebasedatabase.app/",
                new FirebaseOptions { AuthTokenAsyncFactory = () => GetTokenByGoogleServices(), AsAccessToken = true })
        );

        services.AddSingleton<IFirebaseAuthProvider>(
            new FirebaseAuthProvider(new FirebaseConfig(apiKey.ApiKey))
        );

        services.AddScoped<IDatabaseUserProvider, DatabaseUserProvider>();
        services.AddScoped<IAuthUserProvider, AuthUserProvider>();
        services.AddScoped<IDatabaseAccountProvider, DatabaseAccountProvider>();
        services.AddScoped<ICloudMessagingProvider, CloudMessagingProvider>();
        services.AddScoped<IDatabaseTransactionProvider, DatabaseTransactionProvider>();
        services.AddScoped<INotificationHandler, NotificationHandler>();
        services.AddScoped<IDatabaseUserSettingsProvider, DatabaseUserSettingsProvider>();
        
        // services.AddScoped<FirebaseToSqlServerMigrator>();

        var provider = services.BuildServiceProvider();
        
        //TODO: split providers to smaller interfaces to provide only functions that needed and make like container.BindInterfaces() 
        // await provider.GetService<IDatabaseUserProvider>().CreateUserAsync(new User
        //     { Id = "testId", DisplayName = "testUser", Email = "userMail@gmail.com" });

        // await provider.GetService<IDatabaseAccountProvider>().RegisterAccount(
        //     new AccountRegisterRequest(
        //         "testId",
        //         new AccountDataRequest(
        //             "testAccountNumber",
        //             "testName",
        //             4,
        //             4,
        //             2,
        //             "1234567890",
        //             "1234",
        //             "Sberbank",
        //             "asdfasdfasdf")
        //     )
        // );

        // await provider.GetService<INotificationHandler>().HandleNotification(new NotificationHandleRequest(
        //     "testId",
        //     "MIR-1234 14:32 зачисление 2800р Sovcombank Баланс: 16 325.95р"
        // ));

    }

    private static async Task<string> GetTokenByGoogleServices()
    {
        var credential = GoogleCredential
            .FromFile(Path.Combine(PathStorage.FirebaseConfigsPath, PathStorage.FirebaseCloudMessagingJson))
            .CreateScoped("https://www.googleapis.com/auth/userinfo.email",
                "https://www.googleapis.com/auth/firebase.database");

        ITokenAccess c = credential;
        return await c.GetAccessTokenForRequestAsync();
    }
}