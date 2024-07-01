namespace AuthApi.Auth;

public interface IAuthService {
    public string CreateToken(User user);
}