using System.ComponentModel.DataAnnotations;

namespace BeautyBeast.Api.Entities;

public class Review
{
    public int Id { get; set; }

    public int ClientId { get; set; }      // Author of the review
    public User Client { get; set; } = null!;

    public int ArtistId { get; set; }      // Subject of the review
    public User Artist { get; set; } = null!;

    public int? BookingId { get; set; }    // Optional: tied to a specific booking
    public Booking? Booking { get; set; }

    [Range(1, 5)]
    public int Rating { get; set; }

    [Required, MaxLength(2000)]
    public string Text { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
