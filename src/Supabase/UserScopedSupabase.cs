namespace Common.SupabaseAccess;

public sealed class UserScopedSupabase : IUserScopedSupabase
{
    public UserScopedSupabase(SupabaseHostOptions host, IAccessTokenAccessor tokens)
    {
        var token = tokens.AccessToken;
        if (string.IsNullOrWhiteSpace(token))
            throw new UnauthorizedException();

        var options = new SupabaseOptions
        {
            AutoRefreshToken = false,
            AutoConnectRealtime = false
        };
        options.Headers["Authorization"] = "Bearer " + token;
        Client = new Client(host.Url, host.PublishableKey, options);
    }

    public Client Client { get; }
}
