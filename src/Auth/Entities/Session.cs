using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AuthApi.Data;

namespace AuthApi.Auth.Entities;

public class Session : ISoftDelete {
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }

    [Required] [MaxLength(45)] public string IpAddress { get; set; } = default!;
    [Required] [MaxLength(512)] public string UserAgent { get; set; } = default!;

    [Required] [MaxLength(200)] public string RefreshTokenHash { get; set; } = default!;
    public DateTime RefreshTokenExpiresAt { get; set; }

    [Required] [MaxLength(450)] public string UserId { get; set; } = default!;
    public User User { get; set; } = default!;
    public DateTime CreatedAt { get; set; }
    public bool IsDeleted { get; set; }
}