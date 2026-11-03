namespace Common.Context;

public interface IRequestContextAccessor
{
    RequestContext Current { get; set; }
}
