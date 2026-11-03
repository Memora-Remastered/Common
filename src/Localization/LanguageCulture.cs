namespace Common.Localization;

internal static class LanguageCulture
{
    public static string Resolve(string? header)
    {
        return header?.Trim().ToLowerInvariant() switch
        {
            "vi" or "vn" => "vi-VN",
            "en" => "en-US",
            _ => "vi-VN"
        };
    }
}
