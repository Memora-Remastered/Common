namespace Common.Extensions;

public static class HostServiceCollectionExtensions
{
    public static IServiceCollection AddSupabase(this IServiceCollection services, IConfiguration configuration)
    {
        var url = configuration["Supabase:Url"]?.Trim().TrimEnd('/');
        var secret = configuration["Supabase:SecretKey"];
        var publishable = configuration["Supabase:PublishableKey"];
        if (string.IsNullOrWhiteSpace(url) || string.IsNullOrWhiteSpace(secret) || string.IsNullOrWhiteSpace(publishable))
            throw new InvalidOperationException("Supabase:Url, Supabase:SecretKey, and Supabase:PublishableKey are required.");

        services.AddSingleton(new Client(url, secret, new SupabaseOptions
        {
            AutoRefreshToken = false,
            AutoConnectRealtime = false
        }));
        services.AddSingleton(new SupabaseHostOptions { Url = url, PublishableKey = publishable });
        services.AddScoped<IUserScopedSupabase, UserScopedSupabase>();
        services.TryAddSingleton(PaginationConfigs.Create(null));
        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
        return services;
    }

    public static IServiceCollection AddCors(this IServiceCollection services, IConfiguration configuration)
    {
        var origins = (configuration["AllowDomains"] ?? "")
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        services.AddCors(options =>
        {
            options.AddPolicy("AllowDomains", policy =>
            {
                if (origins.Length == 0)
                    return;

                policy.WithOrigins(origins).AllowAnyHeader().AllowAnyMethod().AllowCredentials();
            });
        });

        return services;
    }

    public static IServiceCollection AddPagination(this IServiceCollection services, IConfiguration configuration)
    {
        // Last registration wins. Call this after AddSupabase so the host section overrides the defaults.
        services.AddSingleton(PaginationConfigs.Create(configuration));
        return services;
    }
}
