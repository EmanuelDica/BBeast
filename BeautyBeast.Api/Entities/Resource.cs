using System.ComponentModel.DataAnnotations;

namespace BeautyBeast.Api.Entities;

// Educational resources library (articles, videos, documents).
public class Resource
{
    public int Id { get; set; }

    public int AuthorId { get; set; }
    public User Author { get; set; } = null!;

    [Required, MaxLength(160)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(80)]
    public string? Category { get; set; }       // e.g. "Skin Safety", "Sterilisation", "Aftercare"

    [Required, MaxLength(8000)]
    public string Body { get; set; } = string.Empty;

    [MaxLength(300)]
    public string? MediaUrl { get; set; }

    [MaxLength(300)]
    public string? ExternalUrl { get; set; }

    public ResourceKind Kind { get; set; } = ResourceKind.Article;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
