namespace Common.Context;

public sealed class RequestContext
{
    public string CorrelationId { get; set; } = "";

    public string Language { get; set; } = "";

    public string RequestUrl { get; set; } = "";
}
