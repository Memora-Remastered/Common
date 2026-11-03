namespace Common.Services;

public abstract class BaseService<R>(IStringLocalizer<R> stringLocalizer)
{
    protected IStringLocalizer<R> StringLocalizer { get; } = stringLocalizer;

    protected static Task<TResponse<T>> OkAsync<T>(T data)
    {
        return Task.FromResult(new TResponse<T>(data, ""));
    }

    protected static Task<TResponse<T>> OkAsync<T>(T data, string message)
    {
        return Task.FromResult(new TResponse<T>(data, message));
    }

    protected static Task<TResponse<T>> FailAsync<T>(string status, string message, params object[] arguments)
    {
        return Task.FromResult(Failed<T>(status, Format(message, arguments)));
    }

    protected Task<TResponse<T>> FailAsync<T>(string statusCode, params object[] arguments)
    {
        return Task.FromResult(Failed<T>(statusCode, Localize(statusCode, arguments)));
    }

    private string Localize(string statusCode, object[] arguments)
    {
        var localized = StringLocalizer[statusCode];
        if (localized.ResourceNotFound || localized.Value == statusCode)
            return "Please define message in Resource folder";

        return Format(localized.Value, arguments);
    }

    private static string Format(string message, object[] arguments)
    {
        return arguments.Length > 0 ? string.Format(message, arguments) : message;
    }

    private static TResponse<T> Failed<T>(string status, string message, T? data = default)
    {
        return new TResponse<T>
        {
            IsSuccess = false,
            Status = status,
            Message = message,
            Data = data
        };
    }
}
