using Autofac;
using Autofac.Extensions.DependencyInjection;
using Balance_Support.DataClasses.Records.AccountData;
using Balance_Support.DataClasses.Records.NotificationData;
using Balance_Support.DataClasses.SerializationClasses;
using Balance_Support.Scripts.Controllers;
using Balance_Support.Scripts.Database;
using Balance_Support.Scripts.Database.Providers;
using Balance_Support.Scripts.Database.Providers.Interfaces;
using Balance_Support.Scripts.Extensions.DIExtensions;
using Balance_Support.Scripts.Parsing;
using Balance_Support.Scripts.WebSockets;
using Balance_Support.Scripts.WebSockets.ConnectionManager;
using Balance_Support.Scripts.WebSockets.Interfaces;
using Firebase.Auth;
using Firebase.Database;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using User = Balance_Support.DataClasses.DatabaseEntities.User;

namespace Balance_Support.Scripts.Main.Initializers;

public static class ServicesInitializer
{
  public static async Task Initialize(WebApplicationBuilder builder)
    {
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowSpecificOrigin",
                policy => policy.WithOrigins("http://localhost:5173", "https://balance-support.vercel.app", "https://localhost:7158")
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials());
        });
        
        builder.Services.AddDistributedMemoryCache();

        builder.Services.AddAuthorization();

        builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(options =>
            {
                options.Cookie.HttpOnly = true;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                options.Cookie.SameSite = SameSiteMode.None;
                options.Cookie.Name = "AuthCookie";
                options.Cookie.Domain = ".balancesupportapi.top";
                options.Cookie.Path = "/";
                options.LoginPath = "/account/login";
                options.LogoutPath = "/account/logout";
                options.AccessDeniedPath = "/account/accessdenied";
                options.SlidingExpiration = true;
                options.Cookie.IsEssential = true;
                options.ExpireTimeSpan = TimeSpan.FromDays(7);
            });

        // Session configuration
        builder.Services.AddSession(options =>
        {
            options.IdleTimeout = TimeSpan.FromHours(24);
            options.Cookie.HttpOnly = true;
            options.Cookie.IsEssential = true;
        });

        builder.Services.AddHttpContextAccessor();
        
        // Add SignalR support
        builder.Services.AddSignalR();
        
        builder.Services.AddDataProtection()
            .PersistKeysToDbContext<ApplicationDbContext>() // Persist keys to the database
            .SetDefaultKeyLifetime(TimeSpan.FromDays(90));
        
        builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());
        
        
        // Autofac container builder
        builder.Host.ConfigureContainer<ContainerBuilder>(containerBuilder =>
        {
            containerBuilder.Register(c =>
            {
                var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
                optionsBuilder.UseSqlServer(builder.Configuration.GetConnectionString("SqlServerConnection"));
                return new ApplicationDbContext(optionsBuilder.Options);
            }).AsSelf().InstancePerLifetimeScope();
            
            var apiKey = JsonConvert.DeserializeObject<FirebaseAuthApiKey>(
                File.ReadAllText(Path.Combine(ConstStorage.FirebaseConfigsPath, ConstStorage.FirebaseAuthApiKey)));

            var databaseConfig = JsonConvert.DeserializeObject<FirebaseDatabaseClientConfig>(
                File.ReadAllText(
                    Path.Combine(ConstStorage.FirebaseConfigsPath, ConstStorage.FirebaseDatabaseClientConfigJson)));

            FirebaseApp.Create(new AppOptions
            {
                Credential = GoogleCredential.FromFile(Path.Combine(ConstStorage.FirebaseConfigsPath,
                    ConstStorage.FirebaseCloudMessagingJson))
            });

            // Register Firebase Client
            containerBuilder.Register(c =>
                new FirebaseClient("https://balance-support-b9da3-default-rtdb.europe-west1.firebasedatabase.app/",
                    new FirebaseOptions
                        { AuthTokenAsyncFactory = () => GetTokenByGoogleServices(), AsAccessToken = true })
            ).AsSelf().SingleInstance();

            // Register IFirebaseAuthProvider in Autofac
            containerBuilder.Register(c =>
                new FirebaseAuthProvider(new FirebaseConfig(apiKey.ApiKey))
            ).As<IFirebaseAuthProvider>().SingleInstance();

            containerBuilder.RegisterType<ConnectionManager>()
                .AsImplementedInterfaces()
                .SingleInstance();

            containerBuilder.RegisterType<BaseHub>()
                .AsImplementedInterfaces()
                .SingleInstance()
                .ExternallyOwned();  
            
            containerBuilder.RegisterType<DbContextContainer>()
                .AsImplementedInterfaces()
                .InstancePerLifetimeScope();
            
            // Register services in Autofac
            containerBuilder.RegisterType<DatabaseUserProvider>()
                .AsImplementedInterfaces()
                .InstancePerLifetimeScope();

            containerBuilder.RegisterType<DatabaseUserSettingsProvider>()
                .AsImplementedInterfaces()
                .InstancePerLifetimeScope();

            containerBuilder.RegisterType<DatabaseUserProvider>()
                .AsImplementedInterfaces()
                .InstancePerLifetimeScope();

            containerBuilder.RegisterType<DatabaseAccountProvider>()
                .AsImplementedInterfaces()
                .InstancePerLifetimeScope();

            containerBuilder.RegisterType<DatabaseTransactionProvider>()
                .AsImplementedInterfaces()
                .InstancePerLifetimeScope();

            containerBuilder.RegisterType<NotificationMessageParser>()
                .AsImplementedInterfaces()
                .InstancePerLifetimeScope();
            
            

            containerBuilder.RegisterType<UserController>()
                .AsImplementedInterfaces()
                .InstancePerLifetimeScope();
            
            containerBuilder.RegisterType<UserSettingsController>()
                .AsImplementedInterfaces()
                .InstancePerLifetimeScope();
            
            containerBuilder.RegisterType<AccountsController>()
                .AsImplementedInterfaces()
                .InstancePerLifetimeScope();
            
            containerBuilder.RegisterType<TransactionController>()
                .AsImplementedInterfaces()
                .InstancePerLifetimeScope();
        });

     
        var provider = builder.Services.BuildServiceProvider();
    }

    private static async Task<string> GetTokenByGoogleServices()
    {
        var credential = GoogleCredential
            .FromFile(Path.Combine(ConstStorage.FirebaseConfigsPath, ConstStorage.FirebaseCloudMessagingJson))
            .CreateScoped("https://www.googleapis.com/auth/userinfo.email",
                "https://www.googleapis.com/auth/firebase.database");

        ITokenAccess c = credential;
        return await c.GetAccessTokenForRequestAsync();
    }
}