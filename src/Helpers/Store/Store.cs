using Microsoft.EntityFrameworkCore;

namespace AuthApi.Helpers.Store;

public class Store<TEntity, TDbContext>(TDbContext db) : IStore<TEntity>
    where TEntity : class
    where TDbContext : DbContext {
    protected readonly DbSet<TEntity> DbSet = db.Set<TEntity>();

    public Task<bool> AnyAsync(CancellationToken cancellationToken = default) {
        return DbSet.AnyAsync(cancellationToken);
    }

    public Task<int> CountAsync(CancellationToken cancellationToken = default) {
        return DbSet.CountAsync(cancellationToken);
    }

    public virtual ValueTask<TEntity?> GetByIdsAsync(object?[]? keyValues, bool asNoTracking,
        CancellationToken cancellationToken = default) {
        return DbSet.FindAsync(keyValues, cancellationToken);
    }

    public virtual Task<List<TEntity>> GetAllAsync(bool asNoTracking, CancellationToken cancellationToken = default) {
        return DbSet.ToListAsync(cancellationToken);
    }

    public virtual async Task AddAsync(TEntity entity) {
        await DbSet.AddAsync(entity);
    }

    public virtual Task AddRangeAsync(params TEntity[] entities) {
        return DbSet.AddRangeAsync(entities);
    }

    public virtual Task AddRangeAsync(IEnumerable<TEntity> entities) {
        return DbSet.AddRangeAsync(entities);
    }

    public virtual void Update(TEntity entity) {
        DbSet.Update(entity);
    }

    public virtual void UpdateRange(params TEntity[] entities) {
        DbSet.UpdateRange(entities);
    }

    public virtual void UpdateRange(IEnumerable<TEntity> entities) {
        DbSet.UpdateRange(entities);
    }

    public virtual void Delete(TEntity entity) {
        DbSet.Remove(entity);
    }

    public virtual void DeleteRange(params TEntity[] entities) {
        DbSet.RemoveRange(entities);
    }

    public virtual void DeleteRange(IEnumerable<TEntity> entities) {
        DbSet.RemoveRange(entities);
    }
}