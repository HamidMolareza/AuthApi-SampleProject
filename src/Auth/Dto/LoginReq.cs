using System.ComponentModel.DataAnnotations;

namespace AuthApi.Auth.Dto;

public class LoginReq {
    [Required] [EmailAddress] public required string Email { get; set; }

    [Required]
    [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.",
        MinimumLength = 6)]
    [DataType(DataType.Password)]
    public required string Password { get; set; }
}