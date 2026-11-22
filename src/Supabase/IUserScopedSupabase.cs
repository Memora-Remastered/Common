namespace Common.SupabaseAccess;

public interface IUserScopedSupabase
{
    Client Client { get; }
}
