using BeautyBeast.Api.Entities;

namespace BeautyBeast.Api.Dtos;

public record NotificationDto(
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
    DateTime CreatedAt
);
