using AuthApi.Auth.Entities;
using AuthApi.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace AuthApi.Auth.Services;

public class UserManager(
    AppDbContext db,
    IUserStore<User> store,
    IOptions<IdentityOptions> optionsAccessor,
    IPasswordHasher<User> passwordHasher,
    IEnumerable<IUserValidator<User>> userValidators,
    IEnumerable<IPasswordValidator<User>> passwordValidators,
    ILookupNormalizer keyNormalizer,
    IdentityErrorDescriber errors,
    IServiceProvider services,
    ILogger<UserManager<User>> logger)
    : UserManager<User>(store, optionsAccessor, passwordHasher, userValidators, passwordValidators, keyNormalizer,
        errors, services, logger) {
    public IQueryable<User> UsersWithRoles => Users
        .Include(user => user.UserRoles)
        .ThenInclude(userRoles => userRoles.Role);

    public void RemoveRanges(IEnumerable<User> users) {
        db.Users.RemoveRange(users);
    }

    public void RemoveRanges(params User[] users) {
        db.Users.RemoveRange(users);
    }

    public void Update(User user) {
        db.Users.Update(user);
    }
}