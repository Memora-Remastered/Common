namespace Common.Authentication;

public interface IWorkContext
{
    ICurrentUser User { get; }

    string? CorrelationId { get; }
}
