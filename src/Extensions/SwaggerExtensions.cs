namespace Common.Extensions;

public static class SwaggerExtensions
{
    public static IServiceCollection AddSwagger(this IServiceCollection services)
    {
        services.AddOpenApi(options =>
        {
            options.AddDocumentTransformer((document, context, cancellationToken) =>
            {
                document.Info ??= new();
                document.Info.Title = Assembly.GetEntryAssembly()?.GetName().Name ?? "API";
                return Task.CompletedTask;
            });
            options.AddOperationTransformer((operation, context, cancellationToken) =>
            {
                AddHeader(operation, "Language", "vi, vn, or en. Missing or any other value is vi-VN.");
                AddHeader(operation, "CorrelationId", "Optional GUID. Generated when missing or invalid.");
                return Task.CompletedTask;
            });
        });
        return services;
    }

    public static WebApplication UseSwagger(this WebApplication app)
    {
        if (!app.Environment.IsDevelopment())
            return app;

        app.MapOpenApi();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/openapi/v1.json", app.Environment.ApplicationName);
        });
        return app;
    }

    private static void AddHeader(OpenApiOperation operation, string name, string description)
    {
        operation.Parameters ??= [];
        if (operation.Parameters.Any(parameter => parameter.Name == name && parameter.In == ParameterLocation.Header))
            return;

        operation.Parameters.Add(new OpenApiParameter
        {
            Name = name,
            In = ParameterLocation.Header,
            Required = false,
            Description = description,
            Schema = new OpenApiSchema { Type = JsonSchemaType.String }
        });
    }
}
