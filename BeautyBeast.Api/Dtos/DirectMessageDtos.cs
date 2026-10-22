using System.ComponentModel.DataAnnotations;

namespace BeautyBeast.Api.Dtos;

public record ConversationDto(
    int Id,
    int OtherUserId,
    string OtherUsername,
    string OtherDisplayName,
    string? OtherAvatarUrl,
    string? LastMessagePreview,
    DateTime LastMessageAt,
    int UnreadCount
);

public record DirectMessageDto(
    int Id,
    int ConversationId,
    int SenderId,
    string SenderUsername,
    string SenderDisplayName,
    string? SenderAvatarUrl,
    string Text,
    bool IsRead,
    DateTime CreatedAt
);

public record SendMessageRequest(
    [Required, MaxLength(4000)] string Text
);
