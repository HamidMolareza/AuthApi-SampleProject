using System.ComponentModel.DataAnnotations;

namespace AuthApi.Auth.Dto;

public class RegisterUserReq {
    [Required] [EmailAddress] public required string Email { get; set; }

    [Required]
    [MinLength(3)]
    [MaxLength(110)]
    public required string UserName { get; set; }

    [Required]
    [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.",
        MinimumLength = 6)]
    [DataType(DataType.Password)]
    public required string Password { get; set; }

    [DataType(DataType.Password)]
    [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
    public required string ConfirmPassword { get; set; }
}