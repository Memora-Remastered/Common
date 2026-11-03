namespace Common.Context;

internal static class CorrelationIds
{
    public static string From(HttpContext context)
    {
        var header = context.Request.Headers["CorrelationId"].ToString();
        return Guid.TryParse(header, out var id) ? id.ToString() : context.TraceIdentifier;
    }
}
