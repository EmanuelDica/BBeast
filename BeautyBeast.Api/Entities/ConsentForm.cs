using System.ComponentModel.DataAnnotations;

namespace BeautyBeast.Api.Entities;

// Consent form templates artists can use with their clients.
public class ConsentForm
{
    public int Id { get; set; }

    public int? OwnerId { get; set; }   // Null = built-in template; otherwise owned by an artist
    public User? Owner { get; set; }

    [Required, MaxLength(160)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(80)]
    public string? Category { get; set; }       // e.g. "Microblading", "Lip Filler"

    [Required]
    public string Body { get; set; } = string.Empty;

    public bool IsTemplate { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
