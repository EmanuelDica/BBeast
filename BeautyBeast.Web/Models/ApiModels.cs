using System.Text.Json.Serialization;

namespace BeautyBeast.Web.Models;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum UserRole { Client = 0, Artist = 1, Admin = 2 }

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum BookingStatus { Pending = 0, Confirmed = 1, Cancelled = 2, Completed = 3, Declined = 4 }

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PostKind { Text = 0, Image = 1, Video = 2, Article = 3 }

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ResourceKind { Article = 0, Video = 1, Document = 2, Link = 3 }

public record AuthResponse(
    string Token,
    DateTime ExpiresAt,
    int UserId,
    string Username,
    string DisplayName,
    UserRole Role,
    string? AvatarUrl);

public record CurrentUser(
    int Id,
    string Username,
    string Email,
    string DisplayName,
    UserRole Role,
    string? Bio,
    string? AvatarUrl,
    string? CoverUrl);

public record UserSummary(
    int Id,
    string Username,
    string DisplayName,
    UserRole Role,
    string? AvatarUrl,
    string? Bio);

public record ArtistProfileDto(
    int UserId,
    string Username,
    string DisplayName,
    string? AvatarUrl,
    string? CoverUrl,
    string? Bio,
    string? Headline,
    string? Specialty,
    string? Location,
    string? Pronouns,
    string? About,
    string? Experience,
    string? Education,
    string? Skills,
    string? Website,
    string? Phone,
    bool AcceptingBookings,
    double AverageRating,
    int ReviewCount);

public record ClientProfileDto(
    int UserId,
    string Username,
    string DisplayName,
    string? AvatarUrl,
    string? CoverUrl,
    string? Bio,
    string? Location,
    string? Pronouns,
    string? Interests,
    string? About);

public record Achievement(
    int Id,
    int UserId,
    string Title,
    string? Issuer,
    string? Description,
    DateTime? IssuedOn,
    string? CredentialUrl);

public record Post(
    int Id,
    int AuthorId,
    string AuthorUsername,
    string AuthorDisplayName,
    string? AuthorAvatarUrl,
    UserRole AuthorRole,
    string? Title,
    string Body,
    List<string> MediaUrls,
    PostKind Kind,
    DateTime CreatedAt,
    int LikeCount,
    int CommentCount,
    bool LikedByCurrentUser);

public record StatusItem(
    int Id,
    int AuthorId,
    string AuthorUsername,
    string AuthorDisplayName,
    string? AuthorAvatarUrl,
    UserRole AuthorRole,
    string Text,
    DateTime CreatedAt,
    int LikeCount = 0,
    int CommentCount = 0,
    bool LikedByCurrentUser = false);

public record Comment(
    int Id,
    int PostId,
    int AuthorId,
    string AuthorUsername,
    string AuthorDisplayName,
    string? AuthorAvatarUrl,
    string Text,
    DateTime CreatedAt);

public record Treatment(
    int Id,
    int ArtistId,
    string ArtistUsername,
    string ArtistDisplayName,
    string? ArtistAvatarUrl,
    string Name,
    string? Category,
    string? Description,
    decimal Price,
    int DurationMinutes,
    string? ImageUrl,
    bool IsActive);

public record Booking(
    int Id,
    int ClientId,
    string ClientUsername,
    string ClientDisplayName,
    string? ClientAvatarUrl,
    int ArtistId,
    string ArtistUsername,
    string ArtistDisplayName,
    string? ArtistAvatarUrl,
    int TreatmentId,
    string TreatmentName,
    decimal Price,
    DateTime ScheduledFor,
    int DurationMinutes,
    BookingStatus Status,
    string? ClientNotes,
    string? ArtistNotes,
    DateTime CreatedAt);

public record Review(
    int Id,
    int ClientId,
    string ClientUsername,
    string ClientDisplayName,
    string? ClientAvatarUrl,
    int ArtistId,
    int Rating,
    string Text,
    DateTime CreatedAt,
    int? BookingId);

public record Resource(
    int Id,
    int AuthorId,
    string AuthorUsername,
    string AuthorDisplayName,
    string? AuthorAvatarUrl,
    string Title,
    string? Category,
    string Body,
    string? MediaUrl,
    string? ExternalUrl,
    ResourceKind Kind,
    DateTime CreatedAt);

public record ConsentForm(
    int Id,
    int? OwnerId,
    string? OwnerUsername,
    string Title,
    string? Category,
    string Body,
    bool IsTemplate,
    DateTime UpdatedAt);

public record StatusComment(
    int Id,
    int StatusId,
    int AuthorId,
    string AuthorUsername,
    string AuthorDisplayName,
    string? AuthorAvatarUrl,
    string Text,
    DateTime CreatedAt);

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum NotificationKind
{
    PostLiked = 0,
    PostCommented = 1,
    StatusLiked = 2,
    StatusCommented = 3,
    BookingRequested = 4,
    BookingConfirmed = 5,
    BookingCancelled = 6,
    MessageReceived = 7,
    BookingReminder = 8
}

public record Notification(
    int Id,
    int RecipientId,
    int? ActorId,
    string? ActorUsername,
    string? ActorDisplayName,
    string? ActorAvatarUrl,
    NotificationKind Kind,
    int? PostId,
    int? StatusId,
    int? BookingId,
    string? Message,
    bool IsRead,
    DateTime CreatedAt);

public record SearchResults(
    IReadOnlyList<UserSummary> Artists,
    IReadOnlyList<Treatment> Treatments,
    IReadOnlyList<Resource> Resources);

public record ConversationItem(
    int Id,
    int OtherUserId,
    string OtherUsername,
    string OtherDisplayName,
    string? OtherAvatarUrl,
    string? LastMessagePreview,
    DateTime LastMessageAt,
    int UnreadCount);

public record DirectMessageItem(
    int Id,
    int ConversationId,
    int SenderId,
    string SenderUsername,
    string SenderDisplayName,
    string? SenderAvatarUrl,
    string Text,
    bool IsRead,
    DateTime CreatedAt);

public record SendMessageRequest(string Text);

public record UploadResponse(string Url);

public record WorkingDayDto(int Day, int StartMinutes, int EndMinutes, int? BreakStartMinutes, int? BreakEndMinutes);
public record SetScheduleRequest(List<WorkingDayDto> Days);
public record SlotDto(string Time, int StartMinutes);
public record SlotsResponse(string Date, int TreatmentId, int DurationMinutes, List<SlotDto> Slots, bool DayOff, string? Reason);

// Request payloads
public record RegisterRequest(string Username, string Email, string DisplayName, string Password, UserRole Role);
public record LoginRequest(string UsernameOrEmail, string Password);
public record CreatePostRequest(string? Title, string Body, List<string>? MediaUrls, PostKind Kind);
public record CreateStatusRequest(string Text);
public record CreateStatusCommentRequest(string Text);
public record CreateCommentRequest(string Text);
public record CreateTreatmentRequest(string Name, string? Category, string? Description, decimal Price, int DurationMinutes, string? ImageUrl, bool IsActive);
public record CreateBookingRequest(int TreatmentId, DateTime ScheduledFor, string? ClientNotes);
public record UpdateBookingStatusRequest(BookingStatus Status, string? ArtistNotes);
public record CreateReviewRequest(int ArtistId, int Rating, string Text, int? BookingId);
public record CreateResourceRequest(string Title, string? Category, string Body, string? MediaUrl, string? ExternalUrl, ResourceKind Kind);
public record CreateConsentFormRequest(string Title, string? Category, string Body, bool IsTemplate);
public record CreateAchievementRequest(string Title, string? Issuer, string? Description, DateTime? IssuedOn, string? CredentialUrl);
public record UpdateArtistProfileRequest(
    string? DisplayName, string? Bio, string? AvatarUrl, string? CoverUrl,
    string? Headline, string? Specialty, string? Location, string? Pronouns,
    string? About, string? Experience, string? Education, string? Skills,
    string? Website, string? Phone, bool AcceptingBookings);
public record UpdateClientProfileRequest(
    string? DisplayName, string? Bio, string? AvatarUrl, string? CoverUrl,
    string? Location, string? Pronouns, string? Interests, string? About);
