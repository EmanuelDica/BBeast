namespace BeautyBeast.Api.Entities;

/// <summary>
/// Tracks which reminder notifications have already been dispatched for a booking,
/// so the background service never sends the same reminder twice.
/// </summary>
public class BookingReminderLog
{
    public int Id { get; set; }

    public int BookingId { get; set; }
    public Booking Booking { get; set; } = null!;

    public ReminderInterval Interval { get; set; }

    public DateTime SentAt { get; set; } = DateTime.UtcNow;
}
