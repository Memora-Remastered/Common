namespace Common.Context;

public sealed class RequestContextAccessor : IRequestContextAccessor
{
    private readonly AsyncLocal<RequestContext?> _current = new();

    public RequestContext Current
    {
        get => _current.Value ??= new RequestContext();
        set => _current.Value = value;
    }
}
