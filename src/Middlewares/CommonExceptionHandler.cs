namespace Common.Middlewares;

public sealed class CommonExceptionHandler(
    ILogger<CommonExceptionHandler> logger,
    IConfiguration configuration,
    IStringLocalizer<Resource> localizer) : IExceptionHandler
{
    private readonly ILogger<CommonExceptionHandler> _logger = logger;
    private readonly IConfiguration _configuration = configuration;
    private readonly IStringLocalizer<Resource> _localizer = localizer;

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (httpContext.Response.HasStarted)
            return false;

        var (business, status, errorCode) = Classify(exception);
        var errors = exception is Exceptions.ValidationException validation && validation.Errors.Count > 0
            ? validation.Errors
            : null;
        var correlationId = Context.CorrelationIds.From(httpContext);
        var response = new TResponse<object>
        {
            IsSuccess = false,
            Status = status,
            ErrorCode = errorCode
        };

        if (business)
        {
            response.Message = MessageFor(exception, errors);
            response.Errors = errors?.ToDictionary(pair => pair.Key, pair => pair.Value);
            httpContext.Response.StatusCode = StatusCodes.Status200OK;
        }
        else
        {
            response.Message = correlationId;
            response.DebugDetail = DebugDetail(exception);
            httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
            _logger.LogError(
                exception,
                "CorrelationId {CorrelationId} ErrorCode {ErrorCode} {PublicMessage}",
                correlationId,
                errorCode,
                PublicMessage(httpContext));
        }

        await httpContext.Response.WriteAsJsonAsync(response, cancellationToken);
        return true;
    }

    private string PublicMessage(HttpContext httpContext)
    {
        var culture = CultureInfo.GetCultureInfo(LanguageCulture.Resolve(httpContext.Request.Headers["Language"].ToString()));
        var previous = CultureInfo.CurrentUICulture;
        CultureInfo.CurrentUICulture = culture;
        try
        {
            return _localizer["MessageConfirm.ErrorOccurred"].Value;
        }
        finally
        {
            CultureInfo.CurrentUICulture = previous;
        }
    }

    private string? DebugDetail(Exception exception)
    {
        var raw = _configuration["ExceptionHandling:IncludeDetail"];
        if (!bool.TryParse(raw, out var include) || !include)
            return null;

        var text = $"{exception.GetType().Name}: {exception.Message}";
        return text.Length <= 1024 ? text : text[..1024];
    }

    private static string MessageFor(Exception exception, IDictionary<string, string[]>? errors)
    {
        if (errors is { Count: > 0 })
        {
            var joined = string.Join("; ", errors.SelectMany(error => error.Value).Where(message => !string.IsNullOrWhiteSpace(message)));
            if (joined.Length > 0)
                return joined;
        }

        return exception.Message ?? "";
    }

    private static (bool Business, string Status, string ErrorCode) Classify(Exception exception) => exception switch
    {
        Exceptions.ValidationException => (true, ResponseStatus.InvalidArgument, ErrorCodes.Business.Validation),
        BadRequestException => (true, ResponseStatus.BadRequest, ErrorCodes.Business.BadRequest),
        NotFoundException => (true, ResponseStatus.NotFound, ErrorCodes.Business.NotFound),
        UnauthorizedException => (true, ResponseStatus.Unauthorized, ErrorCodes.Business.Unauthorized),
        ForbiddenException => (true, ResponseStatus.Forbidden, ErrorCodes.Business.Forbidden),
        MethodNotAllowedException => (true, ResponseStatus.MethodNotAllowed, ErrorCodes.Business.MethodNotAllowed),
        _ => (false, ResponseStatus.Error, ErrorCodes.Unknown.Unhandled)
    };
}
