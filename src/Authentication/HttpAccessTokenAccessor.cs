namespace Common.Authentication;

public sealed class HttpAccessTokenAccessor(IHttpContextAccessor http) : IAccessTokenAccessor
{
    public string? AccessToken
    {
        get
        {
            var header = http.HttpContext?.Request.Headers.Authorization.ToString();
            const string prefix = "Bearer ";
            if (string.IsNullOrWhiteSpace(header) || !header.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                return null;

            var token = header[prefix.Length..].Trim();
            return token.Length == 0 ? null : token;
        }
    }
}
