namespace Common.SupabaseAccess;

public sealed class SupabaseHostOptions
{
    public required string Url { get; init; }

    public required string PublishableKey { get; init; }
}
