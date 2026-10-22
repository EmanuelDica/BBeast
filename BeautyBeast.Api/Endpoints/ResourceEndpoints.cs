using BeautyBeast.Api.Data;
using BeautyBeast.Api.Dtos;
using BeautyBeast.Api.Entities;
using BeautyBeast.Api.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BeautyBeast.Api.Endpoints;

public static class ResourceEndpoints
{
    public static IEndpointRouteBuilder MapResourceEndpoints(this IEndpointRouteBuilder app)
    {
        var grp = app.MapGroup("/api/resources").WithTags("Resources");

        grp.MapGet("/", async (AppDbContext db, [FromQuery] string? q, [FromQuery] string? category, [FromQuery] int? authorId) =>
        {
            var query = db.Resources.Include(r => r.Author).AsQueryable();
            if (authorId.HasValue) query = query.Where(r => r.AuthorId == authorId.Value);
            if (!string.IsNullOrWhiteSpace(category)) query = query.Where(r => r.Category == category);
            if (!string.IsNullOrWhiteSpace(q))
            {
                var key = q.Trim().ToLowerInvariant();
                query = query.Where(r =>
                    r.Title.ToLower().Contains(key) ||
                    (r.Category != null && r.Category.ToLower().Contains(key)) ||
                    r.Body.ToLower().Contains(key));
            }
            var list = await query
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => new ResourceDto(
                    r.Id, r.AuthorId, r.Author.Username, r.Author.DisplayName, r.Author.AvatarUrl,
                    r.Title, r.Category, r.Body, r.MediaUrl, r.ExternalUrl, r.Kind, r.CreatedAt))
                .ToListAsync();
            return Results.Ok(list);
        });

        grp.MapGet("/{id:int}", async (int id, AppDbContext db) =>
        {
            var r = await db.Resources.Include(x => x.Author).FirstOrDefaultAsync(x => x.Id == id);
            if (r is null) return Results.NotFound();
            return Results.Ok(new ResourceDto(
                r.Id, r.AuthorId, r.Author.Username, r.Author.DisplayName, r.Author.AvatarUrl,
                r.Title, r.Category, r.Body, r.MediaUrl, r.ExternalUrl, r.Kind, r.CreatedAt));
        });

        grp.MapPost("/", async (HttpContext ctx, [FromBody] CreateResourceRequest req, AppDbContext db) =>
        {
            var id = ctx.User.RequireUserId();
            var user = await db.Users.FindAsync(id);
            if (user is null || user.Role != UserRole.Artist) return Results.Forbid();
            var r = new Resource
            {
                AuthorId = id,
                Title = req.Title.Trim(),
                Category = req.Category,
                Body = req.Body.Trim(),
                MediaUrl = req.MediaUrl,
                ExternalUrl = req.ExternalUrl,
                Kind = req.Kind
            };
            db.Resources.Add(r);
            await db.SaveChangesAsync();
            return Results.Created($"/api/resources/{r.Id}", new ResourceDto(
                r.Id, r.AuthorId, user.Username, user.DisplayName, user.AvatarUrl,
                r.Title, r.Category, r.Body, r.MediaUrl, r.ExternalUrl, r.Kind, r.CreatedAt));
        }).RequireAuthorization();

        grp.MapPut("/{id:int}", async (int id, HttpContext ctx, [FromBody] CreateResourceRequest req, AppDbContext db) =>
        {
            var userId = ctx.User.RequireUserId();
            var r = await db.Resources.FindAsync(id);
            if (r is null) return Results.NotFound();
            if (r.AuthorId != userId) return Results.Forbid();
            r.Title = req.Title.Trim();
            r.Category = req.Category;
            r.Body = req.Body.Trim();
            r.MediaUrl = req.MediaUrl;
            r.ExternalUrl = req.ExternalUrl;
            r.Kind = req.Kind;
            r.UpdatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync();
            return Results.NoContent();
        }).RequireAuthorization();

        grp.MapDelete("/{id:int}", async (int id, HttpContext ctx, AppDbContext db) =>
        {
            var userId = ctx.User.RequireUserId();
            var r = await db.Resources.FindAsync(id);
            if (r is null) return Results.NotFound();
            if (r.AuthorId != userId) return Results.Forbid();
            db.Resources.Remove(r);
            await db.SaveChangesAsync();
            return Results.NoContent();
        }).RequireAuthorization();

        return app;
    }
}
