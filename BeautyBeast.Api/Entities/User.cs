using System.ComponentModel.DataAnnotations;

namespace BeautyBeast.Api.Entities;

public class User
{
    public int Id { get; set; }

    [Required, MaxLength(64)]
    public string Username { get; set; } = string.Empty;

    [Required, MaxLength(256)]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    [Required, MaxLength(80)]
    public string DisplayName { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Bio { get; set; }

    [MaxLength(300)]
    public string? AvatarUrl { get; set; }

    [MaxLength(300)]
    public string? CoverUrl { get; set; }

    public UserRole Role { get; set; } = UserRole.Client;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public ArtistProfile? ArtistProfile { get; set; }
    public ClientProfile? ClientProfile { get; set; }

    public ICollection<Post> Posts { get; set; } = new List<Post>();
    public ICollection<StatusUpdate> Statuses { get; set; } = new List<StatusUpdate>();
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    public ICollection<PostLike> Likes { get; set; } = new List<PostLike>();
    public ICollection<Treatment> Treatments { get; set; } = new List<Treatment>();
    public ICollection<Booking> BookingsAsClient { get; set; } = new List<Booking>();
    public ICollection<Booking> BookingsAsArtist { get; set; } = new List<Booking>();
    public ICollection<Review> ReviewsWritten { get; set; } = new List<Review>();
    public ICollection<Review> ReviewsReceived { get; set; } = new List<Review>();
    public ICollection<Achievement> Achievements { get; set; } = new List<Achievement>();
    public ICollection<Resource> ResourcesAuthored { get; set; } = new List<Resource>();
    public ICollection<WorkingDay> WorkingDays { get; set; } = new List<WorkingDay>();
    public ICollection<StatusLike> StatusLikes { get; set; } = new List<StatusLike>();
    public ICollection<StatusComment> StatusComments { get; set; } = new List<StatusComment>();
    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
}
