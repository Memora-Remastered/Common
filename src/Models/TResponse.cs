namespace Common.Models;

public class TResponse<T>
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public DateTime UtcTime { get; set; } = DateTime.UtcNow;

    public string Status { get; set; } = "";

    public string ErrorCode { get; set; } = "";

    public string Message { get; set; } = "";

    public bool IsSuccess { get; set; }

    public T? Data { get; set; }

    public Dictionary<string, string[]>? Errors { get; set; }

    public string? DebugDetail { get; set; }

    public TResponse()
    {
    }

    public TResponse(T data)
    {
        IsSuccess = true;
        Data = data;
    }

    public TResponse(T data, string message)
    {
        IsSuccess = true;
        Data = data;
        Message = message;
    }

    public static TResponse<T> Success(T data, string message = "")
    {
        return new TResponse<T>(data) { Message = message };
    }

    public static TResponse<T> Fail(
        string status,
        string errorCode,
        string message,
        IDictionary<string, string[]>? errors = null)
    {
        Dictionary<string, string[]>? copy = null;
        if (errors is { Count: > 0 })
            copy = errors.ToDictionary(pair => pair.Key, pair => pair.Value);

        return new TResponse<T>
        {
            IsSuccess = false,
            Status = status,
            ErrorCode = errorCode,
            Message = message,
            Errors = copy
        };
    }
}
