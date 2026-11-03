namespace Common.Extensions;

public static partial class StringExtensions
{
    public static string ToTitleCase(this string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        return System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(input.ToLower());
    }

    public static string ToCamelCase(this string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        if (input.Length == 1)
            return input.ToLower();

        return char.ToLower(input[0]) + input[1..];
    }

    public static string ToPascalCase(this string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        if (input.Length == 1)
            return input.ToUpper();

        return char.ToUpper(input[0]) + input[1..];
    }

    public static string ToSlug(this string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        // Convert to lowercase
        var slug = input.ToLowerInvariant();

        // Replace spaces with hyphens
        slug = SpacesRegex().Replace(slug, "-");

        // Remove invalid characters
        slug = InvalidCharactersRegex().Replace(slug, "");

        // Remove duplicate hyphens
        slug = DuplicateHyphensRegex().Replace(slug, "-");

        // Trim hyphens from start and end
        return slug.Trim('-');
    }

    [GeneratedRegex(@"\s+")]
    private static partial Regex SpacesRegex();
    [GeneratedRegex(@"[^a-z0-9\-]")]
    private static partial Regex InvalidCharactersRegex();
    [GeneratedRegex(@"-+")]
    private static partial Regex DuplicateHyphensRegex();
}