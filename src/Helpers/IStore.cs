namespace AuthApi.Helpers;

public interface IStore<TEntity> where TEntity : class {
    public ValueTask<TEntity?> GetByIdsAsync(object?[]? keyValues, bool asNoTracking,
        CancellationToken cancellationToken = default);

    public Task<List<TEntity>> GetAllAsync(bool asNoTracking, CancellationToken cancellationToken = default);

    public Task AddAsync(TEntity entity);
    public Task AddRangeAsync(params TEntity[] entities);
    public Task AddRangeAsync(IEnumerable<TEntity> entities);

    public void Update(TEntity entity);
    public void UpdateRange(params TEntity[] entities);
    public void UpdateRange(IEnumerable<TEntity> entities);

    public void Delete(TEntity entity);
    public void DeleteRange(params TEntity[] entities);
    public void DeleteRange(IEnumerable<TEntity> entities);
}