using System.ComponentModel.DataAnnotations;
using BeautyBeast.Api.Entities;

namespace BeautyBeast.Api.Dtos;

public record RegisterRequest(
    [Required, MinLength(3), MaxLength(64)] string Username,
    [Required, EmailAddress, MaxLength(256)] string Email,
    [Required, MinLength(2), MaxLength(80)] string DisplayName,
    [Required, MinLength(6), MaxLength(128)] string Password,
    UserRole Role
);

public record LoginRequest(
    [Required] string UsernameOrEmail,
    [Required] string Password
);

public record AuthResponse(
    string Token,
    DateTime ExpiresAt,
    int UserId,
    string Username,
    string DisplayName,
    UserRole Role,
    string? AvatarUrl
);

public record CurrentUserDto(
    int Id,
    string Username,
    string Email,
    string DisplayName,
    UserRole Role,
    string? Bio,
    string? AvatarUrl,
    string? CoverUrl
);
