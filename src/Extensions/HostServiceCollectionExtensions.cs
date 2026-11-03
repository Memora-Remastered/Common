namespace Common.Extensions;

public static class HostServiceCollectionExtensions
{
    public static IServiceCollection AddSupabase(this IServiceCollection services, IConfiguration configuration)
    {
        var url = configuration["Supabase:Url"];
        var key = configuration["Supabase:SecretKey"];
        if (string.IsNullOrWhiteSpace(url) || string.IsNullOrWhiteSpace(key))
            throw new InvalidOperationException("Supabase:Url and Supabase:SecretKey are required.");

        services.AddSingleton(new Client(url, key, new SupabaseOptions
        {
            AutoRefreshToken = false,
            AutoConnectRealtime = false
        }));
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
