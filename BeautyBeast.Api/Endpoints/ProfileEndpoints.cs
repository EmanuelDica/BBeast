using BeautyBeast.Api.Data;
using BeautyBeast.Api.Dtos;
using BeautyBeast.Api.Entities;
using BeautyBeast.Api.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BeautyBeast.Api.Endpoints;

public static class ProfileEndpoints
{
    public static IEndpointRouteBuilder MapProfileEndpoints(this IEndpointRouteBuilder app)
    {
        var grp = app.MapGroup("/api").WithTags("Profiles");

        // List artists (browse)
        grp.MapGet("/artists", async (AppDbContext db, [FromQuery] string? q) =>
        {
            var query = db.Users.Where(u => u.Role == UserRole.Artist);
            if (!string.IsNullOrWhiteSpace(q))
            {
                var qq = q.Trim().ToLower();
                query = query.Where(u =>
                    u.Username.Contains(qq) ||
                    u.DisplayName.ToLower().Contains(qq) ||
                    (u.Bio != null && u.Bio.ToLower().Contains(qq)) ||
                    (u.ArtistProfile != null && (
                        (u.ArtistProfile.Headline != null && u.ArtistProfile.Headline.ToLower().Contains(qq)) ||
                        (u.ArtistProfile.Specialty != null && u.ArtistProfile.Specialty.ToLower().Contains(qq)) ||
                        (u.ArtistProfile.Location != null && u.ArtistProfile.Location.ToLower().Contains(qq)) ||
                        (u.ArtistProfile.Skills != null && u.ArtistProfile.Skills.ToLower().Contains(qq))
                    )));
            }
            var list = await query
                .OrderBy(u => u.DisplayName)
                .Select(u => new UserSummaryDto(u.Id, u.Username, u.DisplayName, u.Role, u.AvatarUrl, u.Bio))
                .ToListAsync();
            return Results.Ok(list);
        });

        // Get artist profile by username
        grp.MapGet("/artists/{username}", async (string username, AppDbContext db) =>
        {
            var key = username.Trim().ToLowerInvariant();
            var user = await db.Users
                .Include(u => u.ArtistProfile)
                .FirstOrDefaultAsync(u => u.Username == key && u.Role == UserRole.Artist);
            if (user is null) return Results.NotFound();

            var reviews = await db.Reviews.Where(r => r.ArtistId == user.Id).ToListAsync();
            var avg = reviews.Count == 0 ? 0d : reviews.Average(r => r.Rating);

            var p = user.ArtistProfile ?? new ArtistProfile();
            return Results.Ok(new ArtistProfileDto(
                user.Id, user.Username, user.DisplayName, user.AvatarUrl, user.CoverUrl, user.Bio,
                p.Headline, p.Specialty, p.Location, p.Pronouns, p.About, p.Experience, p.Education, p.Skills,
                p.Website, p.Phone, p.AcceptingBookings, avg, reviews.Count));
        });

        // Get client profile by username
        grp.MapGet("/clients/{username}", async (string username, AppDbContext db) =>
        {
            var key = username.Trim().ToLowerInvariant();
            var user = await db.Users
                .Include(u => u.ClientProfile)
                .FirstOrDefaultAsync(u => u.Username == key && u.Role == UserRole.Client);
            if (user is null) return Results.NotFound();
            var p = user.ClientProfile ?? new ClientProfile();
            return Results.Ok(new ClientProfileDto(
                user.Id, user.Username, user.DisplayName, user.AvatarUrl, user.CoverUrl, user.Bio,
                p.Location, p.Pronouns, p.Interests, p.About));
        });

        // Update artist profile (self only)
        grp.MapPut("/artists/me", async (HttpContext ctx, [FromBody] UpdateArtistProfileRequest req, AppDbContext db) =>
        {
            var id = ctx.User.RequireUserId();
            var user = await db.Users.Include(u => u.ArtistProfile).FirstOrDefaultAsync(u => u.Id == id);
            if (user is null || user.Role != UserRole.Artist) return Results.Forbid();

            if (!string.IsNullOrWhiteSpace(req.DisplayName)) user.DisplayName = req.DisplayName.Trim();
            if (req.Bio is not null) user.Bio = req.Bio;
            if (req.AvatarUrl is not null) user.AvatarUrl = req.AvatarUrl;
            if (req.CoverUrl is not null) user.CoverUrl = req.CoverUrl;

            user.ArtistProfile ??= new ArtistProfile { UserId = user.Id };
            var p = user.ArtistProfile;
            p.Headline = req.Headline;
            p.Specialty = req.Specialty;
            p.Location = req.Location;
            p.Pronouns = req.Pronouns;
            p.About = req.About;
            p.Experience = req.Experience;
            p.Education = req.Education;
            p.Skills = req.Skills;
            p.Website = req.Website;
            p.Phone = req.Phone;
            p.AcceptingBookings = req.AcceptingBookings;
            p.UpdatedAt = DateTime.UtcNow;

            await db.SaveChangesAsync();
            return Results.NoContent();
        }).RequireAuthorization();

        // Update client profile (self only)
        grp.MapPut("/clients/me", async (HttpContext ctx, [FromBody] UpdateClientProfileRequest req, AppDbContext db) =>
        {
            var id = ctx.User.RequireUserId();
            var user = await db.Users.Include(u => u.ClientProfile).FirstOrDefaultAsync(u => u.Id == id);
            if (user is null || user.Role != UserRole.Client) return Results.Forbid();

            if (!string.IsNullOrWhiteSpace(req.DisplayName)) user.DisplayName = req.DisplayName.Trim();
            if (req.Bio is not null) user.Bio = req.Bio;
            if (req.AvatarUrl is not null) user.AvatarUrl = req.AvatarUrl;
            if (req.CoverUrl is not null) user.CoverUrl = req.CoverUrl;

            user.ClientProfile ??= new ClientProfile { UserId = user.Id };
            var p = user.ClientProfile;
            p.Location = req.Location;
            p.Pronouns = req.Pronouns;
            p.Interests = req.Interests;
            p.About = req.About;
            p.UpdatedAt = DateTime.UtcNow;

            await db.SaveChangesAsync();
            return Results.NoContent();
        }).RequireAuthorization();

        // Achievements: list for a user
        grp.MapGet("/users/{userId:int}/achievements", async (int userId, AppDbContext db) =>
        {
            var list = await db.Achievements
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.IssuedOn)
                .Select(a => new AchievementDto(a.Id, a.UserId, a.Title, a.Issuer, a.Description, a.IssuedOn, a.CredentialUrl))
                .ToListAsync();
            return Results.Ok(list);
        });

        // Achievements: add (self only, artists only)
        grp.MapPost("/achievements", async (HttpContext ctx, [FromBody] CreateAchievementRequest req, AppDbContext db) =>
        {
            var id = ctx.User.RequireUserId();
            var user = await db.Users.FindAsync(id);
            if (user is null || user.Role != UserRole.Artist) return Results.Forbid();

            var a = new Achievement
            {
                UserId = id,
                Title = req.Title.Trim(),
                Issuer = req.Issuer,
                Description = req.Description,
                IssuedOn = req.IssuedOn,
                CredentialUrl = req.CredentialUrl
            };
            db.Achievements.Add(a);
            await db.SaveChangesAsync();
            return Results.Created($"/api/achievements/{a.Id}",
                new AchievementDto(a.Id, a.UserId, a.Title, a.Issuer, a.Description, a.IssuedOn, a.CredentialUrl));
        }).RequireAuthorization();

        // Achievements: delete (self only)
        grp.MapDelete("/achievements/{id:int}", async (int id, HttpContext ctx, AppDbContext db) =>
        {
            var userId = ctx.User.RequireUserId();
            var a = await db.Achievements.FindAsync(id);
            if (a is null) return Results.NotFound();
            if (a.UserId != userId) return Results.Forbid();
            db.Achievements.Remove(a);
            await db.SaveChangesAsync();
            return Results.NoContent();
        }).RequireAuthorization();

        return app;
    }
}
