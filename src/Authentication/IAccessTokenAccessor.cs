namespace Common.Authentication;

public interface IAccessTokenAccessor
{
    string? AccessToken { get; }
}
