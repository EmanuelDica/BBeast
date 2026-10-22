using System.ComponentModel.DataAnnotations;
using BeautyBeast.Api.Entities;

namespace BeautyBeast.Api.Dtos;

public record ResourceDto(
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
    DateTime CreatedAt
);

public record CreateResourceRequest(
    [Required, MaxLength(160)] string Title,
    [MaxLength(80)] string? Category,
    [Required, MaxLength(8000)] string Body,
    [MaxLength(300)] string? MediaUrl,
    [MaxLength(300)] string? ExternalUrl,
    ResourceKind Kind
);

public record ConsentFormDto(
    int Id,
    int? OwnerId,
    string? OwnerUsername,
    string Title,
    string? Category,
    string Body,
    bool IsTemplate,
    DateTime UpdatedAt
);

public record CreateConsentFormRequest(
    [Required, MaxLength(160)] string Title,
    [MaxLength(80)] string? Category,
    [Required] string Body,
    bool IsTemplate
);

public record UploadResponse(string Url);

public record SearchResultsDto(
    IReadOnlyList<UserSummaryDto> Artists,
    IReadOnlyList<TreatmentDto> Treatments,
    IReadOnlyList<ResourceDto> Resources
);
