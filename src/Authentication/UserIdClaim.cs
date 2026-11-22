namespace Common.Authentication;

public static class UserIdClaim
{
    public const string Type = "sub";

    public const string EmailType = "email";

    public static Guid? Parse(ClaimsPrincipal? principal)
    {
        var value = principal?.FindFirst(Type)?.Value;
        if (string.IsNullOrWhiteSpace(value) || !Guid.TryParse(value, out var id))
            return null;

        return id;
    }

    public static string? Email(ClaimsPrincipal? principal)
    {
        var value = principal?.FindFirst(EmailType)?.Value;
        return string.IsNullOrWhiteSpace(value) ? null : value;
    }

    public static Guid Require(ICurrentUser user)
    {
        if (user.UserId is Guid id)
            return id;

        throw new UnauthorizedException();
    }
}
