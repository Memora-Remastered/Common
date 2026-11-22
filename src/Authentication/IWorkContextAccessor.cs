namespace Common.Authentication;

public interface IWorkContextAccessor
{
    IWorkContext Current { get; }
}
