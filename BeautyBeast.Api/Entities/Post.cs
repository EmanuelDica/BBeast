using System.ComponentModel.DataAnnotations;

namespace BeautyBeast.Api.Entities;

// Long-form post (article, image, video). Artists are the primary authors.
public class Post
{
    public int Id { get; set; }

    public int AuthorId { get; set; }
    public User Author { get; set; } = null!;

    [MaxLength(160)]
    public string? Title { get; set; }

    [Required, MaxLength(8000)]
    public string Body { get; set; } = string.Empty;

    public PostKind Kind { get; set; } = PostKind.Text;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<PostMedia> MediaItems { get; set; } = new List<PostMedia>();
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    public ICollection<PostLike> Likes { get; set; } = new List<PostLike>();
}
