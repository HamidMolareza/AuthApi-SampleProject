namespace AuthApi.Admin.Dto;

public record GetUserByIdRes(
    string Id,
    string? UserName,
    string? Email,
    bool EmailConfirmed,
    IEnumerable<string> Roles);