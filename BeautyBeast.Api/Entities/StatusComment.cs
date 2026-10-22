using System.ComponentModel.DataAnnotations;

namespace BeautyBeast.Api.Entities;

public class StatusComment
{
    public int Id { get; set; }

    public int StatusId { get; set; }
    public StatusUpdate Status { get; set; } = null!;

    public int AuthorId { get; set; }
    public User Author { get; set; } = null!;

    [Required, MaxLength(2000)]
    public string Text { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
