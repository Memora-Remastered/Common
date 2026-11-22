namespace Common.Authentication;

public sealed class WorkContext(ICurrentUser user, IRequestContextAccessor request) : IWorkContext
{
    public ICurrentUser User { get; } = user;

    public string? CorrelationId => request.Current.CorrelationId;
}
