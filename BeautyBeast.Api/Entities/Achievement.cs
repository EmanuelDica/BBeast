using System.ComponentModel.DataAnnotations;

namespace BeautyBeast.Api.Entities;

// Artist's CV-style achievements: certifications, awards, courses.
public class Achievement
{
    public int Id { get; set; }

    public int UserId { get; set; }
    public User User { get; set; } = null!;

    [Required, MaxLength(160)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(160)]
    public string? Issuer { get; set; }

    [MaxLength(2000)]
    public string? Description { get; set; }

    public DateTime? IssuedOn { get; set; }

    [MaxLength(300)]
    public string? CredentialUrl { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
