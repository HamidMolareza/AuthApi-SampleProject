using System.ComponentModel.DataAnnotations;

namespace AuthApi.Auth.Dto;

public class ChangePasswordReq {
    [Required] public required string CurrentPassword { get; set; }

    [Required]
    [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.",
        MinimumLength = 6)]
    [DataType(DataType.Password)]
    public required string NewPassword { get; set; }

    [DataType(DataType.Password)]
    [Compare(nameof(NewPassword), ErrorMessage = "The password and confirmation password do not match.")]
    public required string ConfirmPassword { get; set; }
}