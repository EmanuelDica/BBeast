using System.ComponentModel.DataAnnotations;

namespace BeautyBeast.Api.Entities;

public class Treatment
{
    public int Id { get; set; }

    public int ArtistId { get; set; }
    public User Artist { get; set; } = null!;

    [Required, MaxLength(120)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(80)]
    public string? Category { get; set; }       // e.g. "Lashes", "Brows", "Skin"

    [MaxLength(2000)]
    public string? Description { get; set; }

    public decimal Price { get; set; }

    public int DurationMinutes { get; set; } = 60;

    [MaxLength(300)]
    public string? ImageUrl { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}
