namespace Common.Pipelines;

public class UnhandledExceptionPipeline<TRequest, TResponse>(ILogger<UnhandledExceptionPipeline<TRequest, TResponse>> logger) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<UnhandledExceptionPipeline<TRequest, TResponse>> _logger = logger;

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        try
        {
            return await next(cancellationToken);
        }
        catch (Exception ex) when (!IsExpected(ex))
        {
            var requestName = typeof(TRequest).Name;

            _logger.LogError(ex, "Unhandled exception for request {RequestName}", requestName);

            throw;
        }
    }

    private static bool IsExpected(Exception exception) =>
        exception is Exceptions.ValidationException
            or BadRequestException
            or UnauthorizedException
            or ForbiddenException
            or NotFoundException
            or MethodNotAllowedException
            or ArgumentException;
}