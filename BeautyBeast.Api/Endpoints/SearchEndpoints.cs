using BeautyBeast.Api.Data;
using BeautyBeast.Api.Dtos;
using BeautyBeast.Api.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BeautyBeast.Api.Endpoints;

public static class SearchEndpoints
{
    public static IEndpointRouteBuilder MapSearchEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/search", async (AppDbContext db, [FromQuery] string q) =>
        {
            if (string.IsNullOrWhiteSpace(q))
                return Results.Ok(new SearchResultsDto(
                    Array.Empty<UserSummaryDto>(), Array.Empty<TreatmentDto>(), Array.Empty<ResourceDto>()));

            var key = q.Trim().ToLowerInvariant();

            var artists = await db.Users
                .Where(u => u.Role == UserRole.Artist &&
                    (u.Username.Contains(key) ||
                     u.DisplayName.ToLower().Contains(key) ||
                     (u.Bio != null && u.Bio.ToLower().Contains(key)) ||
                     (u.ArtistProfile != null && (
                         (u.ArtistProfile.Specialty != null && u.ArtistProfile.Specialty.ToLower().Contains(key)) ||
                         (u.ArtistProfile.Skills != null && u.ArtistProfile.Skills.ToLower().Contains(key)) ||
                         (u.ArtistProfile.Location != null && u.ArtistProfile.Location.ToLower().Contains(key))))))
                .OrderBy(u => u.DisplayName)
                .Take(20)
                .Select(u => new UserSummaryDto(u.Id, u.Username, u.DisplayName, u.Role, u.AvatarUrl, u.Bio))
                .ToListAsync();

            var treatments = await db.Treatments.Include(t => t.Artist)
                .Where(t => t.IsActive && (
                    t.Name.ToLower().Contains(key) ||
                    (t.Category != null && t.Category.ToLower().Contains(key)) ||
                    (t.Description != null && t.Description.ToLower().Contains(key))))
                .OrderBy(t => t.Name)
                .Take(20)
                .Select(t => new TreatmentDto(
                    t.Id, t.ArtistId, t.Artist.Username, t.Artist.DisplayName, t.Artist.AvatarUrl,
                    t.Name, t.Category, t.Description, t.Price, t.DurationMinutes, t.ImageUrl, t.IsActive))
                .ToListAsync();

            var resources = await db.Resources.Include(r => r.Author)
                .Where(r => r.Title.ToLower().Contains(key) ||
                            (r.Category != null && r.Category.ToLower().Contains(key)) ||
                            r.Body.ToLower().Contains(key))
                .OrderByDescending(r => r.CreatedAt)
                .Take(20)
                .Select(r => new ResourceDto(
                    r.Id, r.AuthorId, r.Author.Username, r.Author.DisplayName, r.Author.AvatarUrl,
                    r.Title, r.Category, r.Body, r.MediaUrl, r.ExternalUrl, r.Kind, r.CreatedAt))
                .ToListAsync();

            return Results.Ok(new SearchResultsDto(artists, treatments, resources));
        }).WithTags("Search");

        return app;
    }
}
