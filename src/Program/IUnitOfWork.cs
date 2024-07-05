using AuthApi.Auth.Entities;
using AuthApi.Auth.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Storage;

namespace AuthApi.Program;

public interface IUnitOfWork {
    public UserManager UserManager { get; }
    public RoleManager<Role> RoleManager { get; }
    public ITokenManager TokenManager { get; }
    public ISessionManager SessionManager { get; }
    public SignInManager<User> SignInManager { get; }
    public Task<IDbContextTransaction> BeginTransactionAsync();
    public Task<int> SaveChangesAsync();
}