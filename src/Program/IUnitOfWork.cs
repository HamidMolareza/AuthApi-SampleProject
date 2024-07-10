using AuthApi.Auth.Services.Role;
using AuthApi.Auth.Services.Session;
using AuthApi.Auth.Services.UserServices;

namespace AuthApi.Program;

public interface IUnitOfWork : IDisposable {
    public IRoleStore RoleStore { get; }
    public IUserStore UserStore { get; }
    public ISessionStore SessionStore { get; }
    public Task BeginTransactionAsync();
    public Task CommitTransactionAsync();
    public Task RollbackTransactionAsync();
    public Task<int> SaveChangesAsync();
}