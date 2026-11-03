namespace Common.Models;

public sealed class PaginationConfigs
{
    public const string SectionName = "PaginationConfigs";

    public int DefaultPageIndex { get; set; } = 1;

    public int DefaultPageSize { get; set; } = 15;

    public int MaxPageSize { get; set; } = 100;

    public static PaginationConfigs Create(IConfiguration? configuration)
    {
        var configs = new PaginationConfigs();
        configuration?.GetSection(SectionName).Bind(configs);

        if (configs.DefaultPageIndex < 1)
            configs.DefaultPageIndex = 1;

        if (configs.DefaultPageSize < 1)
            configs.DefaultPageSize = 15;

        if (configs.MaxPageSize < configs.DefaultPageSize)
            configs.MaxPageSize = configs.DefaultPageSize;

        return configs;
    }

    public (int Page, int PageSize) Clamp(int page, int pageSize)
    {
        if (page < DefaultPageIndex)
            page = DefaultPageIndex;

        if (pageSize < 1)
            pageSize = DefaultPageSize;

        if (pageSize > MaxPageSize)
            pageSize = MaxPageSize;

        return (page, pageSize);
    }
}
