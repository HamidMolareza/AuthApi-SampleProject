namespace AuthApi.Auth;

public interface IAuthService {
    public Task<string> CreateTokenAsync(User user);
}