namespace BeautyBeast.Api.Entities;

/// <summary>
/// A direct-message thread between exactly two users.
/// ParticipantAId is always the lower user ID so (A,B) and (B,A) map to the same row.
/// </summary>
public class Conversation
{
    public int Id { get; set; }

    public int ParticipantAId { get; set; }   // lower  user ID
    public User ParticipantA { get; set; } = null!;

    public int ParticipantBId { get; set; }   // higher user ID
    public User ParticipantB { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime LastMessageAt { get; set; } = DateTime.UtcNow;

    public ICollection<DirectMessage> Messages { get; set; } = new List<DirectMessage>();
}
