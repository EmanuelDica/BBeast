using System.ComponentModel.DataAnnotations;

namespace BeautyBeast.Api.Entities;

public class Booking
{
    public int Id { get; set; }

    public int ClientId { get; set; }
    public User Client { get; set; } = null!;

    public int ArtistId { get; set; }
    public User Artist { get; set; } = null!;

    public int TreatmentId { get; set; }
    public Treatment Treatment { get; set; } = null!;

    public DateTime ScheduledFor { get; set; }
    public int DurationMinutes { get; set; } = 60;

    public BookingStatus Status { get; set; } = BookingStatus.Pending;

    [MaxLength(1000)]
    public string? ClientNotes { get; set; }

    [MaxLength(1000)]
    public string? ArtistNotes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
