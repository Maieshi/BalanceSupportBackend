namespace Balance_Support.Scripts.Extensions.DIExtensions;
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInterfacesTransient<TService>(this IServiceCollection services)
        where TService : class
    {
        // Get all the interfaces implemented by the TService class
        var interfaces = typeof(TService).GetInterfaces();

        // Register each interface to the DI container with a scoped lifetime
        foreach (var @interface in interfaces)
        {
            services.AddTransient(@interface, typeof(TService));
        }

        return services;
    }
    
    public static IServiceCollection AddInterfacesScoped<TService>(this IServiceCollection services)
        where TService : class
    {
        // Get all the interfaces implemented by the TService class
        var interfaces = typeof(TService).GetInterfaces();

        // Register each interface to the DI container with a scoped lifetime
        foreach (var @interface in interfaces)
        {
            services.AddScoped(@interface, typeof(TService));
        }

        return services;
    }

    public static IServiceCollection AddInterfacesSingleton<TService>(this IServiceCollection services)
        where TService : class
    {
        // Get all the interfaces implemented by the TService class
        var interfaces = typeof(TService).GetInterfaces();

        // Register each interface to the DI container with a scoped lifetime
        foreach (var @interface in interfaces)
        {
            services.AddSingleton(@interface, typeof(TService));
        }

        return services;
    }
}
