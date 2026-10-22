using System.ComponentModel.DataAnnotations;

namespace BeautyBeast.Api.Entities;

// LinkedIn-style "profile as a CV" for beauty professionals.
public class ArtistProfile
{
    public int Id { get; set; }

    public int UserId { get; set; }
    public User User { get; set; } = null!;

    [MaxLength(120)]
    public string? Headline { get; set; }            // e.g. "Master Aesthetician, London"

    [MaxLength(160)]
    public string? Specialty { get; set; }           // e.g. "Microblading, Lash Lift"

    [MaxLength(160)]
    public string? Location { get; set; }            // City / area

    [MaxLength(80)]
    public string? Pronouns { get; set; }

    [MaxLength(2000)]
    public string? About { get; set; }               // Long-form bio

    [MaxLength(2000)]
    public string? Experience { get; set; }          // Career experience text

    [MaxLength(2000)]
    public string? Education { get; set; }           // Training, courses

    [MaxLength(1000)]
    public string? Skills { get; set; }              // Comma-separated tags

    [MaxLength(300)]
    public string? Website { get; set; }

    [MaxLength(80)]
    public string? Phone { get; set; }

    public bool AcceptingBookings { get; set; } = true;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
