namespace AuthApi.Auth.Dto;

public class GetSessionRes {
    public Guid Id { get; init; }
    public string IpAddress { get; init; } = default!;
    public string UserAgent { get; init; } = default!;
    public DateTime CreatedAt { get; init; }
}