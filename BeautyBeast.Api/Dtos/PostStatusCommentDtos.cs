using System.ComponentModel.DataAnnotations;
using BeautyBeast.Api.Entities;

namespace BeautyBeast.Api.Dtos;

public record PostDto(
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
    bool LikedByCurrentUser
);

public record CreatePostRequest(
    [MaxLength(160)] string? Title,
    [Required, MaxLength(8000)] string Body,
    List<string>? MediaUrls,
    PostKind Kind
);

public record StatusDto(
    int Id,
    int AuthorId,
    string AuthorUsername,
    string AuthorDisplayName,
    string? AuthorAvatarUrl,
    UserRole AuthorRole,
    string Text,
    DateTime CreatedAt,
    int LikeCount,
    int CommentCount,
    bool LikedByCurrentUser
);

public record CreateStatusRequest(
    [Required, MaxLength(500)] string Text
);

public record CommentDto(
    int Id,
    int PostId,
    int AuthorId,
    string AuthorUsername,
    string AuthorDisplayName,
    string? AuthorAvatarUrl,
    string Text,
    DateTime CreatedAt
);

public record CreateCommentRequest(
    [Required, MaxLength(2000)] string Text
);

public record StatusCommentDto(
    int Id,
    int StatusId,
    int AuthorId,
    string AuthorUsername,
    string AuthorDisplayName,
    string? AuthorAvatarUrl,
    string Text,
    DateTime CreatedAt
);
