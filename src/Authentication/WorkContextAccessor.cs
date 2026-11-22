namespace Common.Authentication;

public sealed class WorkContextAccessor(IWorkContext current) : IWorkContextAccessor
{
    public IWorkContext Current { get; } = current;
}
