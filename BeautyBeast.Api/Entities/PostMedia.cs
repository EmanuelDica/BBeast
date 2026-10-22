using System.ComponentModel.DataAnnotations;

namespace BeautyBeast.Api.Entities;

public class PostMedia
{
    public int Id { get; set; }

    public int PostId { get; set; }
    public Post Post { get; set; } = null!;

    [Required, MaxLength(300)]
    public string Url { get; set; } = string.Empty;

    public int SortOrder { get; set; } = 0;
}
