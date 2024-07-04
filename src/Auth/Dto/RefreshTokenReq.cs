namespace AuthApi.Auth.Dto;

public record RefreshTokenReq(string UserId, string RefreshToken);