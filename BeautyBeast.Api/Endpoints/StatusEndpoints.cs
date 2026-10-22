using BeautyBeast.Api.Data;
using BeautyBeast.Api.Dtos;
using BeautyBeast.Api.Entities;
using BeautyBeast.Api.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BeautyBeast.Api.Endpoints;

public static class StatusEndpoints
{
    public static IEndpointRouteBuilder MapStatusEndpoints(this IEndpointRouteBuilder app)
    {
        var grp = app.MapGroup("/api/statuses").WithTags("Statuses");

        grp.MapGet("/", async (HttpContext ctx, AppDbContext db, [FromQuery] int? authorId, [FromQuery] string? author, [FromQuery] int? take) =>
        {
            var currentId = ctx.User.UserId();
            var q = db.Statuses
                .Include(s => s.Author)
                .Include(s => s.Likes)
                .Include(s => s.Comments)
                .AsQueryable();
            if (authorId.HasValue) q = q.Where(s => s.AuthorId == authorId.Value);
            if (!string.IsNullOrWhiteSpace(author))
            {
                var key = author.Trim().ToLowerInvariant();
                q = q.Where(s => s.Author.Username == key);
            }
            var list = await q.OrderByDescending(s => s.CreatedAt)
                .Take(take is > 0 and <= 200 ? take.Value : 50)
                .ToListAsync();
            return Results.Ok(list.Select(s => ToDto(s, currentId)).ToList());
        });

        grp.MapPost("/", async (HttpContext ctx, [FromBody] CreateStatusRequest req, AppDbContext db) =>
        {
            var id = ctx.User.RequireUserId();
            var user = await db.Users.FindAsync(id);
            if (user is null) return Results.Unauthorized();
            var s = new StatusUpdate { AuthorId = id, Text = req.Text.Trim() };
            db.Statuses.Add(s);
            await db.SaveChangesAsync();
            return Results.Created($"/api/statuses/{s.Id}", ToDto(s, id, user));
        }).RequireAuthorization();

        grp.MapPut("/{id:int}", async (int id, HttpContext ctx, [FromBody] CreateStatusRequest req, AppDbContext db) =>
        {
            var userId = ctx.User.RequireUserId();
            var s = await db.Statuses.FindAsync(id);
            if (s is null) return Results.NotFound();
            if (s.AuthorId != userId) return Results.Forbid();
            s.Text = req.Text.Trim();
            s.UpdatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync();
            return Results.NoContent();
        }).RequireAuthorization();

        grp.MapDelete("/{id:int}", async (int id, HttpContext ctx, AppDbContext db) =>
        {
            var userId = ctx.User.RequireUserId();
            var s = await db.Statuses.FindAsync(id);
            if (s is null) return Results.NotFound();
            if (s.AuthorId != userId) return Results.Forbid();
            db.Statuses.Remove(s);
            await db.SaveChangesAsync();
            return Results.NoContent();
        }).RequireAuthorization();

        // ── Likes ──────────────────────────────────────────────────────────────

        grp.MapPost("/{id:int}/like", async (int id, HttpContext ctx, AppDbContext db) =>
        {
            var userId = ctx.User.RequireUserId();
            var status = await db.Statuses.FindAsync(id);
            if (status is null) return Results.NotFound();

            var existing = await db.StatusLikes.FirstOrDefaultAsync(l => l.StatusId == id && l.UserId == userId);
            if (existing is not null) return Results.Ok(new { liked = true });

            db.StatusLikes.Add(new StatusLike { StatusId = id, UserId = userId });

            // Notify the author (skip self-likes)
            if (status.AuthorId != userId)
            {
                db.Notifications.Add(new Notification
                {
                    RecipientId = status.AuthorId,
                    ActorId = userId,
                    Kind = NotificationKind.StatusLiked,
                    StatusId = id
                });
            }

            await db.SaveChangesAsync();
            return Results.Ok(new { liked = true });
        }).RequireAuthorization();

        grp.MapDelete("/{id:int}/like", async (int id, HttpContext ctx, AppDbContext db) =>
        {
            var userId = ctx.User.RequireUserId();
            var existing = await db.StatusLikes.FirstOrDefaultAsync(l => l.StatusId == id && l.UserId == userId);
            if (existing is not null)
            {
                db.StatusLikes.Remove(existing);
                await db.SaveChangesAsync();
            }
            return Results.Ok(new { liked = false });
        }).RequireAuthorization();

        // ── Comments ───────────────────────────────────────────────────────────

        grp.MapGet("/{id:int}/comments", async (int id, AppDbContext db) =>
        {
            var list = await db.StatusComments
                .Where(c => c.StatusId == id)
                .Include(c => c.Author)
                .OrderBy(c => c.CreatedAt)
                .Select(c => new StatusCommentDto(
                    c.Id, c.StatusId, c.AuthorId,
                    c.Author.Username, c.Author.DisplayName, c.Author.AvatarUrl,
                    c.Text, c.CreatedAt))
                .ToListAsync();
            return Results.Ok(list);
        });

        grp.MapPost("/{id:int}/comments", async (int id, HttpContext ctx, [FromBody] CreateCommentRequest req, AppDbContext db) =>
        {
            var userId = ctx.User.RequireUserId();
            var status = await db.Statuses.FindAsync(id);
            if (status is null) return Results.NotFound();

            var c = new StatusComment { StatusId = id, AuthorId = userId, Text = req.Text.Trim() };
            db.StatusComments.Add(c);

            // Notify the author (skip self-comments)
            if (status.AuthorId != userId)
            {
                db.Notifications.Add(new Notification
                {
                    RecipientId = status.AuthorId,
                    ActorId = userId,
                    Kind = NotificationKind.StatusCommented,
                    StatusId = id
                });
            }

            await db.SaveChangesAsync();
            var author = await db.Users.FindAsync(userId);
            return Results.Created($"/api/statuses/{id}/comments/{c.Id}",
                new StatusCommentDto(c.Id, c.StatusId, c.AuthorId,
                    author!.Username, author.DisplayName, author.AvatarUrl,
                    c.Text, c.CreatedAt));
        }).RequireAuthorization();

        grp.MapDelete("/comments/{commentId:int}", async (int commentId, HttpContext ctx, AppDbContext db) =>
        {
            var userId = ctx.User.RequireUserId();
            var c = await db.StatusComments.FindAsync(commentId);
            if (c is null) return Results.NotFound();
            if (c.AuthorId != userId) return Results.Forbid();
            db.StatusComments.Remove(c);
            await db.SaveChangesAsync();
            return Results.NoContent();
        }).RequireAuthorization();

        return app;
    }

    private static StatusDto ToDto(StatusUpdate s, int? currentUserId, User? author = null)
    {
        var a = author ?? s.Author;
        return new StatusDto(
            s.Id, s.AuthorId,
            a.Username, a.DisplayName, a.AvatarUrl, a.Role,
            s.Text, s.CreatedAt,
            s.Likes.Count, s.Comments.Count,
            currentUserId.HasValue && s.Likes.Any(l => l.UserId == currentUserId.Value));
    }
}
