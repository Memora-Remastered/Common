namespace Common.Authentication;

public sealed class JwtAuthOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = "";

    public string Audience { get; set; } = "authenticated";

    public string JwksUrl { get; set; } = "";

    public string SigningKey { get; set; } = "";

    public int ClockSkewSeconds { get; set; } = 30;
}
