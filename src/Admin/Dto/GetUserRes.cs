namespace AuthApi.Admin.Dto;

public class GetUserRes {
    public GetUserRes() { }

    public GetUserRes(string id,
        string? userName,
        string? email,
        bool emailConfirmed,
        List<string> roles) {
        Id = id;
        UserName = userName;
        Email = email;
        EmailConfirmed = emailConfirmed;
        Roles = roles;
    }

    public string Id { get; init; }
    public string? UserName { get; init; }
    public string? Email { get; init; }
    public bool EmailConfirmed { get; init; }
    public List<string> Roles { get; init; }
}