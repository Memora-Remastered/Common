namespace Common.Repositories;

public class GenericRepository<T>(Client supabaseClient, PaginationConfigs pagination, IRequestContextAccessor request) : IGenericRepository<T> where T : BaseEntity, new()
{
    protected readonly Client _supabaseClient = supabaseClient;
    private readonly PaginationConfigs _pagination = pagination;
    private readonly IRequestContextAccessor _request = request;

    public virtual async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var response = await _supabaseClient
            .From<T>()
            .Where(x => x.Id == id)
            .Limit(1)
            .Get(cancellationToken);

        return response.Models.FirstOrDefault();
    }

    public virtual async Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var response = await _supabaseClient
            .From<T>()
            .Get(cancellationToken);

        return response.Models;
    }

    public virtual async Task<PagingResponse<T>> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        (page, pageSize) = _pagination.Clamp(page, pageSize);
        var offset = (page - 1) * pageSize;

        var response = await _supabaseClient
            .From<T>()
            .Range(offset, offset + pageSize - 1)
            .Get(cancellationToken, Supabase.Postgrest.Constants.CountType.Exact);

        return ToPage(response.Models, response.Count, page, pageSize);
    }

    public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
    {
        var response = await _supabaseClient
            .From<T>()
            .Where(predicate)
            .Get(cancellationToken);

        return response.Models;
    }

    public virtual async Task<PagingResponse<T>> FindPagedAsync(Expression<Func<T, bool>> predicate, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        (page, pageSize) = _pagination.Clamp(page, pageSize);
        var offset = (page - 1) * pageSize;

        var response = await _supabaseClient
            .From<T>()
            .Where(predicate)
            .Range(offset, offset + pageSize - 1)
            .Get(cancellationToken, Supabase.Postgrest.Constants.CountType.Exact);

        return ToPage(response.Models, response.Count, page, pageSize);
    }

    public virtual async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
    {
        var response = await _supabaseClient
            .From<T>()
            .Where(predicate)
            .Limit(1)
            .Get(cancellationToken);

        return response.Models.FirstOrDefault();
    }

    public virtual async Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        // shortcut: a client id is inserted when empty, remove this when the column default should win
        if (entity.Id == Guid.Empty)
            entity.Id = Guid.NewGuid();

        var response = await _supabaseClient
            .From<T>()
            .Insert(entity, cancellationToken: cancellationToken);

        return response.Models[0];
    }

    public virtual async Task<T> UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        var response = await _supabaseClient
            .From<T>()
            .Where(x => x.Id == entity.Id)
            .Update(entity, cancellationToken: cancellationToken);

        return response.Models[0];
    }

    public virtual async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _ = await _supabaseClient
            .From<T>()
            .Where(x => x.Id == id)
            .Delete(cancellationToken: cancellationToken);
    }

    public virtual async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var count = await CountAsync(x => x.Id == id, cancellationToken);
        return count != 0;
    }

    public virtual async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
    {
        var count = await CountAsync(predicate, cancellationToken);
        return count != 0;
    }

    public virtual Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        return _supabaseClient
            .From<T>()
            .Count(Supabase.Postgrest.Constants.CountType.Exact, cancellationToken);
    }

    public virtual Task<int> CountAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return _supabaseClient
            .From<T>()
            .Where(predicate)
            .Count(Supabase.Postgrest.Constants.CountType.Exact, cancellationToken);
    }

    private PagingResponse<T> ToPage(IReadOnlyList<T>? rows, int total, int page, int pageSize)
    {
        return new PagingResponse<T>(rows, total, page, pageSize, _request.Current.RequestUrl);
    }
}