using System.ComponentModel.DataAnnotations;
using BeautyBeast.Api.Entities;

namespace BeautyBeast.Api.Dtos;

public record TreatmentDto(
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
    bool IsActive
);

public record CreateTreatmentRequest(
    [Required, MaxLength(120)] string Name,
    [MaxLength(80)] string? Category,
    [MaxLength(2000)] string? Description,
    [Range(0, 100000)] decimal Price,
    [Range(5, 600)] int DurationMinutes,
    [MaxLength(300)] string? ImageUrl,
    bool IsActive
);

public record BookingDto(
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
    DateTime CreatedAt
);

public record CreateBookingRequest(
    [Required] int TreatmentId,
    [Required] DateTime ScheduledFor,
    [MaxLength(1000)] string? ClientNotes
);

public record UpdateBookingStatusRequest(
    BookingStatus Status,
    [MaxLength(1000)] string? ArtistNotes
);

public record ReviewDto(
    int Id,
    int ClientId,
    string ClientUsername,
    string ClientDisplayName,
    string? ClientAvatarUrl,
    int ArtistId,
    int Rating,
    string Text,
    DateTime CreatedAt,
    int? BookingId
);

public record CreateReviewRequest(
    [Required] int ArtistId,
    [Range(1, 5)] int Rating,
    [Required, MaxLength(2000)] string Text,
    int? BookingId
);
