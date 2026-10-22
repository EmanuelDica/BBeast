using System.ComponentModel.DataAnnotations;

namespace BeautyBeast.Api.Entities;

public class DirectMessage
{
    public int Id { get; set; }

    public int ConversationId { get; set; }
    public Conversation Conversation { get; set; } = null!;

    public int SenderId { get; set; }
    public User Sender { get; set; } = null!;

    [Required, MaxLength(4000)]
    public string Text { get; set; } = string.Empty;

    public bool IsRead { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
