namespace Common.Authentication;

public sealed class HttpCurrentUser(IHttpContextAccessor http) : ICurrentUser
{
    public bool IsAuthenticated => http.HttpContext?.User.Identity?.IsAuthenticated is true;

    public Guid? UserId => UserIdClaim.Parse(http.HttpContext?.User);

    public string? Email => UserIdClaim.Email(http.HttpContext?.User);
}
