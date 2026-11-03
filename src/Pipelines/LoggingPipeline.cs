namespace Common.Pipelines;

public class LoggingPipeline<TRequest, TResponse>(ILogger<LoggingPipeline<TRequest, TResponse>> logger) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<LoggingPipeline<TRequest, TResponse>> _logger = logger;

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;

        _logger.LogInformation("Handling {RequestName}", requestName);

        var response = await next(cancellationToken);

        _logger.LogInformation("Handled {RequestName}", requestName);

        return response;
    }
}