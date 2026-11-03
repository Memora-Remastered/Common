namespace Common.Context;

public sealed class RequestContextMiddleware(RequestDelegate next)
{
    private readonly RequestDelegate _next = next;

    public async Task InvokeAsync(HttpContext context, IRequestContextAccessor accessor)
    {
        accessor.Current = new RequestContext
        {
            CorrelationId = CorrelationIds.From(context),
            Language = context.Request.Headers["Language"].ToString(),
            // shortcut: Kestrel's own scheme/host, add forwarded-headers when this sits behind a proxy
            RequestUrl = context.Request.GetEncodedUrl()
        };

        await _next(context);
    }
}
