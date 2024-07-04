namespace AuthApi.Auth.Dto;

public record TokensRes(
    string AccessToken,
    DateTime AccessTokenExpire,
    string RefreshToken,
    DateTime RefreshTokenExpire);