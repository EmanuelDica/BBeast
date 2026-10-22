using System.ComponentModel.DataAnnotations;
using BeautyBeast.Api.Entities;

namespace BeautyBeast.Api.Dtos;

public record UserSummaryDto(
    int Id,
    string Username,
    string DisplayName,
    UserRole Role,
    string? AvatarUrl,
    string? Bio
);

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
    int ReviewCount
);

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
    string? About
);

public record UpdateUserRequest(
    [MaxLength(80)] string? DisplayName,
    [MaxLength(500)] string? Bio,
    [MaxLength(300)] string? AvatarUrl,
    [MaxLength(300)] string? CoverUrl
);

public record UpdateArtistProfileRequest(
    [MaxLength(80)] string? DisplayName,
    [MaxLength(500)] string? Bio,
    [MaxLength(300)] string? AvatarUrl,
    [MaxLength(300)] string? CoverUrl,
    [MaxLength(120)] string? Headline,
    [MaxLength(160)] string? Specialty,
    [MaxLength(160)] string? Location,
    [MaxLength(80)] string? Pronouns,
    [MaxLength(2000)] string? About,
    [MaxLength(2000)] string? Experience,
    [MaxLength(2000)] string? Education,
    [MaxLength(1000)] string? Skills,
    [MaxLength(300)] string? Website,
    [MaxLength(80)] string? Phone,
    bool AcceptingBookings
);

public record UpdateClientProfileRequest(
    [MaxLength(80)] string? DisplayName,
    [MaxLength(500)] string? Bio,
    [MaxLength(300)] string? AvatarUrl,
    [MaxLength(300)] string? CoverUrl,
    [MaxLength(160)] string? Location,
    [MaxLength(80)] string? Pronouns,
    [MaxLength(1000)] string? Interests,
    [MaxLength(2000)] string? About
);

public record AchievementDto(
    int Id,
    int UserId,
    string Title,
    string? Issuer,
    string? Description,
    DateTime? IssuedOn,
    string? CredentialUrl
);

public record CreateAchievementRequest(
    [Required, MaxLength(160)] string Title,
    [MaxLength(160)] string? Issuer,
    [MaxLength(2000)] string? Description,
    DateTime? IssuedOn,
    [MaxLength(300)] string? CredentialUrl
);
