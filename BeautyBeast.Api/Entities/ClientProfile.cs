using System.ComponentModel.DataAnnotations;

namespace BeautyBeast.Api.Entities;

// Facebook-timeline-style profile for clients (deliberately diminished functionality).
public class ClientProfile
{
    public int Id { get; set; }

    public int UserId { get; set; }
    public User User { get; set; } = null!;

    [MaxLength(160)]
    public string? Location { get; set; }

    [MaxLength(80)]
    public string? Pronouns { get; set; }

    [MaxLength(1000)]
    public string? Interests { get; set; }            // Comma-separated tags

    [MaxLength(2000)]
    public string? About { get; set; }

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
