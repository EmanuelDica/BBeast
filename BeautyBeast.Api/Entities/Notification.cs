namespace BeautyBeast.Api.Entities;

public class Notification
{
    public int Id { get; set; }

    /// <summary>The user who receives this notification.</summary>
    public int RecipientId { get; set; }
    public User Recipient { get; set; } = null!;

    /// <summary>The user who triggered the notification (null for system notifications).</summary>
    public int? ActorId { get; set; }
    public User? Actor { get; set; }

    public NotificationKind Kind { get; set; }

    /// <summary>Optional: the post / status / booking that triggered this.</summary>
    public int? PostId { get; set; }
    public int? StatusId { get; set; }
    public int? BookingId { get; set; }

    /// <summary>Optional detail shown in the notification bell (e.g. booking date/time).</summary>
    public string? Message { get; set; }

    public bool IsRead { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
