namespace Common.Extensions;

public static class CommonServiceCollectionExtensions
{
    public static IServiceCollection AddCommon(this IServiceCollection services, Assembly applicationAssembly)
    {
        ArgumentNullException.ThrowIfNull(applicationAssembly);

        services.AddSingleton<IDateTimeService, DateTimeService>();
        services.AddScoped<IRequestContextAccessor, RequestContextAccessor>();

        services.AddMediatR(configuration =>
        {
            configuration.RegisterServicesFromAssembly(applicationAssembly);
            configuration.AddOpenBehavior(typeof(UnhandledExceptionPipeline<,>));
            configuration.AddOpenBehavior(typeof(LoggingPipeline<,>));
            configuration.AddOpenBehavior(typeof(ValidationPipeline<,>));
        });

        services.AddValidatorsFromAssembly(applicationAssembly);
        services.AddExceptionHandler<CommonExceptionHandler>();
        services.AddProblemDetails();
        services.AddLocalization(options => options.ResourcesPath = "Resources");
        AddCultures(services);

        services.Configure<ApiBehaviorOptions>(options => options.SuppressModelStateInvalidFilter = true);
        services.Configure<MvcOptions>(options => options.Filters.Add(new ModelStateEnvelopeFilter()));
        services.Configure<JsonOptions>(options => EnvelopeJson.Apply(options.JsonSerializerOptions));
        services.Configure<HttpJsonOptions>(options => EnvelopeJson.Apply(options.SerializerOptions));

        return services;
    }

    public static IApplicationBuilder UseCommon(this IApplicationBuilder app)
    {
        return app.UseExceptionHandler();
    }

    private static void AddCultures(IServiceCollection services)
    {
        var supported = new[] { new CultureInfo("vi-VN"), new CultureInfo("en-US") };
        services.Configure<RequestLocalizationOptions>(options =>
        {
            options.DefaultRequestCulture = new RequestCulture("vi-VN");
            options.SupportedCultures = supported;
            options.SupportedUICultures = supported;
            options.ApplyCurrentCultureToResponseHeaders = true;
            options.RequestCultureProviders.Insert(0, new CustomRequestCultureProvider(context =>
            {
                var culture = LanguageCulture.Resolve(context.Request.Headers["Language"].ToString());
                return Task.FromResult<ProviderCultureResult?>(new ProviderCultureResult(culture, culture));
            }));
        });
    }
}
