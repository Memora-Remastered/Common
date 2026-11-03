namespace Common.Models;

public sealed class PagingResponse<T> where T : class
{
    public int TotalRecords { get; }

    public int CurrentPageNumber { get; }

    public int PageSize { get; }

    public int TotalPages { get; }

    public bool HasNextPage { get; }

    public bool HasPreviousPage { get; }

    public string? NextPage { get; }

    public string? PreviousPage { get; }

    public IReadOnlyList<T> Data { get; }

    public PagingResponse(IReadOnlyList<T>? data, int totalRecords, int currentPageNumber, int pageSize, string? requestUrl = null)
    {
        Data = data ?? Array.Empty<T>();
        TotalRecords = totalRecords;
        CurrentPageNumber = currentPageNumber;
        PageSize = pageSize;
        TotalPages = pageSize == 0 ? 0 : (int)Math.Ceiling(totalRecords / (double)pageSize);
        HasNextPage = currentPageNumber < TotalPages;
        HasPreviousPage = currentPageNumber > 1;
        NextPage = HasNextPage ? WithPage(requestUrl, currentPageNumber + 1) : null;
        PreviousPage = HasPreviousPage ? WithPage(requestUrl, currentPageNumber - 1) : null;
    }

    private static string? WithPage(string? requestUrl, int page)
    {
        if (string.IsNullOrWhiteSpace(requestUrl) || !Uri.TryCreate(requestUrl, UriKind.Absolute, out var uri))
            return null;

        var query = QueryHelpers.ParseQuery(uri.Query);
        var pageKey = "page";
        foreach (var key in query.Keys)
        {
            if (key.Equals("page", StringComparison.OrdinalIgnoreCase))
            {
                pageKey = key;
                break;
            }
        }

        query[pageKey] = page.ToString(CultureInfo.InvariantCulture);
        return uri.GetLeftPart(UriPartial.Path) + QueryString.Create(query);
    }
}
