namespace BeautyBeast.Api.Entities;

public class StatusLike
{
    public int Id { get; set; }

    public int StatusId { get; set; }
    public StatusUpdate Status { get; set; } = null!;

    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
