using System.Reflection;
using AuthApi.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;

namespace AuthApi.Helpers;

public static class DbContextHelpers {
    /// <summary>
    /// Apply global filter for all entities implementing ISoftDelete
    /// </summary>
    public static void SetSoftDeleteForEntities(this ModelBuilder builder) {
        foreach (var entityType in builder.Model.GetEntityTypes()) {
            if (typeof(ISoftDelete).IsAssignableFrom(entityType.ClrType)) {
                SetSoftDeleteFilter(builder, entityType);
            }
        }
    }

    private static void SetSoftDeleteFilter(ModelBuilder builder, IMutableEntityType entityType) {
        var method = typeof(DbContextHelpers)
            .GetMethod(nameof(SetSoftDeleteFilter),
                BindingFlags.Public | BindingFlags.Static)
            ?.MakeGenericMethod(entityType.ClrType);

        method?.Invoke(null, [builder]);
    }

    public static void SetSoftDeleteFilter<TEntity>(ModelBuilder modelBuilder) where TEntity : class, ISoftDelete {
        modelBuilder.Entity<TEntity>().HasQueryFilter(e => !e.IsDeleted);
    }

    public static void HandleSoftDelete(this ChangeTracker changeTracker) {
        var targetEntities = changeTracker.Entries()
            .Where(e => e is { State: EntityState.Deleted, Entity: ISoftDelete });

        foreach (var entry in targetEntities) {
            entry.State = EntityState.Modified;
            ((ISoftDelete)entry.Entity).IsDeleted = true;
        }
    }
}