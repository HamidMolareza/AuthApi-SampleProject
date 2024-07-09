namespace AuthApi.Auth.Dto;

public record GetSessionRes(
    Guid Id,
    string IpAddress,
    string UserAgent,
    bool IsRevoked,
    DateTime CreatedAt
);