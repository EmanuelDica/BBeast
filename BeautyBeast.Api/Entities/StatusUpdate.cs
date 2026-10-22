using System.ComponentModel.DataAnnotations;

namespace BeautyBeast.Api.Entities;

// Short status update ("what's on your mind?"). Lighter weight than Post.
public class StatusUpdate
{
    public int Id { get; set; }

    public int AuthorId { get; set; }
    public User Author { get; set; } = null!;

    [Required, MaxLength(500)]
    public string Text { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public ICollection<StatusLike> Likes { get; set; } = new List<StatusLike>();
    public ICollection<StatusComment> Comments { get; set; } = new List<StatusComment>();
}
