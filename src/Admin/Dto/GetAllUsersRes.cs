namespace AuthApi.Admin.Dto;

public record GetAllUsersRes(
    string Id,
    string? UserName,
    string? Email,
    bool EmailConfirmed,
    IEnumerable<string> Roles);