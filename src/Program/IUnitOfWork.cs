using AuthApi.Auth.Services.Role;

namespace AuthApi.Program;

public interface IUnitOfWork : IDisposable {
    public IRoleStore RoleStore { get; }
    public Task BeginTransactionAsync();
    public Task CommitTransactionAsync();
    public Task RollbackTransactionAsync();
    public Task<int> SaveChangesAsync();
}