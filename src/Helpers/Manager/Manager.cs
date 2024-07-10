namespace AuthApi.Helpers.Manager;

public class Manager<TStore, TEntity>(TStore store) : IManager<TEntity>
    where TEntity : class
    where TStore : IStore<TEntity> {
    public Task<bool> AnyAsync(CancellationToken cancellationToken = default)
        => store.AnyAsync(cancellationToken);

    public Task<int> CountAsync(CancellationToken cancellationToken = default)
        => store.CountAsync(cancellationToken);

    public ValueTask<TEntity?> GetByIdsAsync(object?[]? keyValues, bool asNoTracking,
        CancellationToken cancellationToken = default)
        => store.GetByIdsAsync(keyValues, asNoTracking, cancellationToken);

    public Task<List<TEntity>> GetAllAsync(bool asNoTracking, CancellationToken cancellationToken = default)
        => store.GetAllAsync(asNoTracking, cancellationToken);

    public Task AddAsync(TEntity entity)
        => store.AddAsync(entity);

    public Task AddRangeAsync(params TEntity[] entities)
        => store.AddRangeAsync(entities);

    public Task AddRangeAsync(IEnumerable<TEntity> entities)
        => store.AddRangeAsync(entities);

    public void Update(TEntity entity)
        => store.Update(entity);

    public void UpdateRange(params TEntity[] entities)
        => store.UpdateRange(entities);

    public void UpdateRange(IEnumerable<TEntity> entities)
        => store.UpdateRange(entities);

    public void Delete(TEntity entity)
        => store.Delete(entity);

    public void DeleteRange(params TEntity[] entities)
        => store.DeleteRange(entities);

    public void DeleteRange(IEnumerable<TEntity> entities)
        => store.DeleteRange(entities);
}