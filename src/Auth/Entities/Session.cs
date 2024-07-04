using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AuthApi.Auth.Entities;

public class Session {
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }

    [Required] [MaxLength(45)] public string IpAddress { get; set; } = default!;
    [Required] [MaxLength(512)] public string UserAgent { get; set; } = default!;

    [Required] [MaxLength(200)] public string RefreshToken { get; set; } = default!;
    public DateTime RefreshTokenExpiresAt { get; set; }

    [Required] [MaxLength(450)] public string UserId { get; set; } = default!;
    public User User { get; set; } = default!;

    public bool IsRevoked { get; set; }
    public DateTime CreatedAt { get; set; }

    public bool Active => !IsRevoked;
}