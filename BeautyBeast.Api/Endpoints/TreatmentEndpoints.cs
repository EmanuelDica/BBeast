using BeautyBeast.Api.Data;
using BeautyBeast.Api.Dtos;
using BeautyBeast.Api.Entities;
using BeautyBeast.Api.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BeautyBeast.Api.Endpoints;

public static class TreatmentEndpoints
{
    public static IEndpointRouteBuilder MapTreatmentEndpoints(this IEndpointRouteBuilder app)
    {
        var grp = app.MapGroup("/api/treatments").WithTags("Treatments");

        grp.MapGet("/", async (AppDbContext db, [FromQuery] int? artistId, [FromQuery] string? q, [FromQuery] string? category) =>
        {
            var query = db.Treatments.Include(t => t.Artist).Where(t => t.IsActive);
            if (artistId.HasValue) query = query.Where(t => t.ArtistId == artistId.Value);
            if (!string.IsNullOrWhiteSpace(category)) query = query.Where(t => t.Category == category);
            if (!string.IsNullOrWhiteSpace(q))
            {
                var key = q.Trim().ToLowerInvariant();
                query = query.Where(t =>
                    t.Name.ToLower().Contains(key) ||
                    (t.Category != null && t.Category.ToLower().Contains(key)) ||
                    (t.Description != null && t.Description.ToLower().Contains(key)));
            }
            var list = await query
                .OrderBy(t => t.Name)
                .Select(t => new TreatmentDto(
                    t.Id, t.ArtistId, t.Artist.Username, t.Artist.DisplayName, t.Artist.AvatarUrl,
                    t.Name, t.Category, t.Description, t.Price, t.DurationMinutes, t.ImageUrl, t.IsActive))
                .ToListAsync();
            return Results.Ok(list);
        });

        grp.MapGet("/{id:int}", async (int id, AppDbContext db) =>
        {
            var t = await db.Treatments.Include(x => x.Artist).FirstOrDefaultAsync(x => x.Id == id);
            if (t is null) return Results.NotFound();
            return Results.Ok(new TreatmentDto(
                t.Id, t.ArtistId, t.Artist.Username, t.Artist.DisplayName, t.Artist.AvatarUrl,
                t.Name, t.Category, t.Description, t.Price, t.DurationMinutes, t.ImageUrl, t.IsActive));
        });

        grp.MapPost("/", async (HttpContext ctx, [FromBody] CreateTreatmentRequest req, AppDbContext db) =>
        {
            var id = ctx.User.RequireUserId();
            var user = await db.Users.FindAsync(id);
            if (user is null || user.Role != UserRole.Artist) return Results.Forbid();
            var t = new Treatment
            {
                ArtistId = id,
                Name = req.Name.Trim(),
                Category = req.Category,
                Description = req.Description,
                Price = req.Price,
                DurationMinutes = req.DurationMinutes,
                ImageUrl = req.ImageUrl,
                IsActive = req.IsActive
            };
            db.Treatments.Add(t);
            await db.SaveChangesAsync();
            return Results.Created($"/api/treatments/{t.Id}", new TreatmentDto(
                t.Id, t.ArtistId, user.Username, user.DisplayName, user.AvatarUrl,
                t.Name, t.Category, t.Description, t.Price, t.DurationMinutes, t.ImageUrl, t.IsActive));
        }).RequireAuthorization();

        grp.MapPut("/{id:int}", async (int id, HttpContext ctx, [FromBody] CreateTreatmentRequest req, AppDbContext db) =>
        {
            var userId = ctx.User.RequireUserId();
            var t = await db.Treatments.FindAsync(id);
            if (t is null) return Results.NotFound();
            if (t.ArtistId != userId) return Results.Forbid();
            t.Name = req.Name.Trim();
            t.Category = req.Category;
            t.Description = req.Description;
            t.Price = req.Price;
            t.DurationMinutes = req.DurationMinutes;
            t.ImageUrl = req.ImageUrl;
            t.IsActive = req.IsActive;
            await db.SaveChangesAsync();
            return Results.NoContent();
        }).RequireAuthorization();

        grp.MapDelete("/{id:int}", async (int id, HttpContext ctx, AppDbContext db) =>
        {
            var userId = ctx.User.RequireUserId();
            var t = await db.Treatments.FindAsync(id);
            if (t is null) return Results.NotFound();
            if (t.ArtistId != userId) return Results.Forbid();
            db.Treatments.Remove(t);
            await db.SaveChangesAsync();
            return Results.NoContent();
        }).RequireAuthorization();

        // Distinct list of categories (handy for filters)
        grp.MapGet("/categories", async (AppDbContext db) =>
        {
            var cats = await db.Treatments
                .Where(t => t.Category != null && t.IsActive)
                .Select(t => t.Category!)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();
            return Results.Ok(cats);
        });

        return app;
    }
}
