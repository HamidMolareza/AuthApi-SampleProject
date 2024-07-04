namespace AuthApi.Auth.Dto;

public record GetSessionsRes(
    Guid Id,
    string IpAddress,
    string UserAgent,
    bool IsRevoked,
    DateTime CreatedAt
);