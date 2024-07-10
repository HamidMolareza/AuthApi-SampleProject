namespace AuthApi.Helpers.Store;

public interface IStore<TEntity> where TEntity : class {
    Task<bool> AnyAsync(CancellationToken cancellationToken = default);
    Task<int> CountAsync(CancellationToken cancellationToken = default);

    ValueTask<TEntity?> GetByIdsAsync(object?[]? keyValues, bool asNoTracking,
        CancellationToken cancellationToken = default);

    Task<List<TEntity>> GetAllAsync(bool asNoTracking, CancellationToken cancellationToken = default);

    Task AddAsync(TEntity entity);
    Task AddRangeAsync(params TEntity[] entities);
    Task AddRangeAsync(IEnumerable<TEntity> entities);

    void Update(TEntity entity);
    void UpdateRange(params TEntity[] entities);
    void UpdateRange(IEnumerable<TEntity> entities);

    void Delete(TEntity entity);
    void DeleteRange(params TEntity[] entities);
    void DeleteRange(IEnumerable<TEntity> entities);
}