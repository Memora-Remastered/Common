namespace Common.Extensions;

public static class JwtAuthenticationExtensions
{
    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, HttpCurrentUser>();
        services.AddScoped<IWorkContext, WorkContext>();
        services.AddScoped<IWorkContextAccessor, WorkContextAccessor>();
        services.AddScoped<IAccessTokenAccessor, HttpAccessTokenAccessor>();

        var jwt = ResolveJwt(configuration);
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options => ConfigureBearer(options, jwt));

        _ = services.AddAuthorization(options =>
        {
            options.AddPolicy(AuthorizationPolicies.Authenticated, policy => policy.RequireAuthenticatedUser());
            options.FallbackPolicy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build();
        });

        return services;
    }

    internal static JwtAuthOptions ResolveJwt(IConfiguration configuration)
    {
        var jwt = new JwtAuthOptions();
        configuration.GetSection(JwtAuthOptions.SectionName).Bind(jwt);

        var supabaseUrl = configuration["Supabase:Url"]?.Trim().TrimEnd('/');
        if (string.IsNullOrWhiteSpace(jwt.Issuer) && !string.IsNullOrWhiteSpace(supabaseUrl))
            jwt.Issuer = $"{supabaseUrl}/auth/v1";

        if (string.IsNullOrWhiteSpace(jwt.Audience))
            jwt.Audience = "authenticated";

        var hasSigningKey = !string.IsNullOrWhiteSpace(jwt.SigningKey);
        if (!hasSigningKey && string.IsNullOrWhiteSpace(jwt.JwksUrl) && !string.IsNullOrWhiteSpace(supabaseUrl))
            jwt.JwksUrl = $"{supabaseUrl}/auth/v1/.well-known/jwks.json";

        if (string.IsNullOrWhiteSpace(jwt.Issuer) || string.IsNullOrWhiteSpace(jwt.Audience))
            throw new InvalidOperationException("Jwt:Issuer and Jwt:Audience are required.");

        if (!hasSigningKey && string.IsNullOrWhiteSpace(jwt.JwksUrl))
            throw new InvalidOperationException("Set Jwt:SigningKey for HS256 or Jwt:JwksUrl for asymmetric keys.");

        if (jwt.ClockSkewSeconds is < 0 or > 120)
            jwt.ClockSkewSeconds = 30;

        return jwt;
    }

    private static void ConfigureBearer(JwtBearerOptions options, JwtAuthOptions jwt)
    {
        options.MapInboundClaims = false;
        options.IncludeErrorDetails = false;
        options.RequireHttpsMetadata = jwt.Issuer.StartsWith("https://", StringComparison.OrdinalIgnoreCase);
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwt.Issuer,
            ValidateAudience = true,
            ValidAudience = jwt.Audience,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            RequireSignedTokens = true,
            RequireExpirationTime = true,
            ClockSkew = TimeSpan.FromSeconds(jwt.ClockSkewSeconds),
            NameClaimType = UserIdClaim.Type,
            RoleClaimType = "app_role"
        };

        if (!string.IsNullOrWhiteSpace(jwt.SigningKey))
        {
            options.TokenValidationParameters.IssuerSigningKey =
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SigningKey));
            options.TokenValidationParameters.ValidAlgorithms = [SecurityAlgorithms.HmacSha256];
        }
        else
        {
            var retriever = new HttpDocumentRetriever
            {
                RequireHttps = jwt.JwksUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase)
            };
            options.ConfigurationManager = new ConfigurationManager<OpenIdConnectConfiguration>(
                jwt.JwksUrl,
                new JwksConfigurationRetriever(),
                retriever);
        }

        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = context =>
            {
                if (UserIdClaim.Parse(context.Principal) is null)
                    context.Fail("User identifier claim is missing or malformed.");
                return Task.CompletedTask;
            }
        };
    }

    private sealed class JwksConfigurationRetriever : IConfigurationRetriever<OpenIdConnectConfiguration>
    {
        public async Task<OpenIdConnectConfiguration> GetConfigurationAsync(
            string address,
            IDocumentRetriever retriever,
            CancellationToken cancel)
        {
            var json = await retriever.GetDocumentAsync(address, cancel);
            var keys = new JsonWebKeySet(json);
            var config = new OpenIdConnectConfiguration { JsonWebKeySet = keys };
            foreach (var key in keys.GetSigningKeys())
                config.SigningKeys.Add(key);
            return config;
        }
    }
}
