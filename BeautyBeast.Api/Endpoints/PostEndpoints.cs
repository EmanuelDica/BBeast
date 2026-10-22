using BeautyBeast.Api.Data;
using BeautyBeast.Api.Dtos;
using BeautyBeast.Api.Entities;
using BeautyBeast.Api.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BeautyBeast.Api.Endpoints;

public static class PostEndpoints
{
    public static IEndpointRouteBuilder MapPostEndpoints(this IEndpointRouteBuilder app)
    {
        var grp = app.MapGroup("/api/posts").WithTags("Posts");

        // Feed (most recent first). Optional ?authorId or ?author=username filter.
        grp.MapGet("/", async (HttpContext ctx, AppDbContext db,
            [FromQuery] int? authorId, [FromQuery] string? author, [FromQuery] int? take) =>
        {
            var currentId = ctx.User.UserId();
            var q = db.Posts
                .Include(p => p.Author)
                .Include(p => p.MediaItems)
                .Include(p => p.Likes)
                .Include(p => p.Comments)
                .AsQueryable();
            if (authorId.HasValue) q = q.Where(p => p.AuthorId == authorId.Value);
            if (!string.IsNullOrWhiteSpace(author))
            {
                var key = author.Trim().ToLowerInvariant();
                q = q.Where(p => p.Author.Username == key);
            }
            var posts = await q.OrderByDescending(p => p.CreatedAt)
                .Take(take is > 0 and <= 200 ? take.Value : 50)
                .ToListAsync();
            return Results.Ok(posts.Select(p => ToDto(p, currentId)).ToList());
        });

        grp.MapGet("/{id:int}", async (int id, HttpContext ctx, AppDbContext db) =>
        {
            var currentId = ctx.User.UserId();
            var p = await db.Posts
                .Include(x => x.Author)
                .Include(x => x.MediaItems)
                .Include(x => x.Likes)
                .Include(x => x.Comments)
                .FirstOrDefaultAsync(x => x.Id == id);
            if (p is null) return Results.NotFound();
            return Results.Ok(ToDto(p, currentId));
        });

        grp.MapPost("/", async (HttpContext ctx, [FromBody] CreatePostRequest req, AppDbContext db) =>
        {
            var id = ctx.User.RequireUserId();
            var user = await db.Users.FindAsync(id);
            if (user is null) return Results.Unauthorized();
            var p = new Post
            {
                AuthorId = id,
                Title = string.IsNullOrWhiteSpace(req.Title) ? null : req.Title.Trim(),
                Body = req.Body.Trim(),
                Kind = req.Kind,
                MediaItems = (req.MediaUrls ?? new())
                    .Where(u => !string.IsNullOrWhiteSpace(u))
                    .Select((u, i) => new PostMedia { Url = u.Trim(), SortOrder = i })
                    .ToList()
            };
            db.Posts.Add(p);
            await db.SaveChangesAsync();
            // Reload author for DTO
            await db.Entry(p).Reference(x => x.Author).LoadAsync();
            return Results.Created($"/api/posts/{p.Id}", ToDto(p, id));
        }).RequireAuthorization();

        grp.MapPut("/{id:int}", async (int id, HttpContext ctx, [FromBody] CreatePostRequest req, AppDbContext db) =>
        {
            var userId = ctx.User.RequireUserId();
            var p = await db.Posts
                .Include(x => x.MediaItems)
                .FirstOrDefaultAsync(x => x.Id == id);
            if (p is null) return Results.NotFound();
            if (p.AuthorId != userId) return Results.Forbid();

            p.Title = string.IsNullOrWhiteSpace(req.Title) ? null : req.Title.Trim();
            p.Body = req.Body.Trim();
            p.Kind = req.Kind;
            p.UpdatedAt = DateTime.UtcNow;

            // Replace media items
            db.PostMediaItems.RemoveRange(p.MediaItems);
            p.MediaItems = (req.MediaUrls ?? new())
                .Where(u => !string.IsNullOrWhiteSpace(u))
                .Select((u, i) => new PostMedia { PostId = id, Url = u.Trim(), SortOrder = i })
                .ToList();

            await db.SaveChangesAsync();
            return Results.NoContent();
        }).RequireAuthorization();

        grp.MapDelete("/{id:int}", async (int id, HttpContext ctx, AppDbContext db) =>
        {
            var userId = ctx.User.RequireUserId();
            var p = await db.Posts.FindAsync(id);
            if (p is null) return Results.NotFound();
            if (p.AuthorId != userId) return Results.Forbid();
            db.Posts.Remove(p);
            await db.SaveChangesAsync();
            return Results.NoContent();
        }).RequireAuthorization();

        // Likes
        grp.MapPost("/{id:int}/like", async (int id, HttpContext ctx, AppDbContext db) =>
        {
            var userId = ctx.User.RequireUserId();
            var post = await db.Posts.FindAsync(id);
            if (post is null) return Results.NotFound();
            var existing = await db.PostLikes.FirstOrDefaultAsync(l => l.PostId == id && l.UserId == userId);
            if (existing is not null) return Results.Ok(new { liked = true });
            db.PostLikes.Add(new PostLike { PostId = id, UserId = userId });

            // Notify author (skip self-likes)
            if (post.AuthorId != userId)
            {
                db.Notifications.Add(new Notification
                {
                    RecipientId = post.AuthorId,
                    ActorId = userId,
                    Kind = NotificationKind.PostLiked,
                    PostId = id
                });
            }

            await db.SaveChangesAsync();
            return Results.Ok(new { liked = true });
        }).RequireAuthorization();

        grp.MapDelete("/{id:int}/like", async (int id, HttpContext ctx, AppDbContext db) =>
        {
            var userId = ctx.User.RequireUserId();
            var existing = await db.PostLikes.FirstOrDefaultAsync(l => l.PostId == id && l.UserId == userId);
            if (existing is not null)
            {
                db.PostLikes.Remove(existing);
                await db.SaveChangesAsync();
            }
            return Results.Ok(new { liked = false });
        }).RequireAuthorization();

        // Comments
        grp.MapGet("/{id:int}/comments", async (int id, AppDbContext db) =>
        {
            var list = await db.Comments
                .Where(c => c.PostId == id)
                .Include(c => c.Author)
                .OrderBy(c => c.CreatedAt)
                .Select(c => new CommentDto(c.Id, c.PostId, c.AuthorId, c.Author.Username, c.Author.DisplayName, c.Author.AvatarUrl, c.Text, c.CreatedAt))
                .ToListAsync();
            return Results.Ok(list);
        });

        grp.MapPost("/{id:int}/comments", async (int id, HttpContext ctx, [FromBody] CreateCommentRequest req, AppDbContext db) =>
        {
            var userId = ctx.User.RequireUserId();
            var post = await db.Posts.FindAsync(id);
            if (post is null) return Results.NotFound();
            var c = new Comment { PostId = id, AuthorId = userId, Text = req.Text.Trim() };
            db.Comments.Add(c);

            // Notify author (skip self-comments)
            if (post.AuthorId != userId)
            {
                db.Notifications.Add(new Notification
                {
                    RecipientId = post.AuthorId,
                    ActorId = userId,
                    Kind = NotificationKind.PostCommented,
                    PostId = id
                });
            }

            await db.SaveChangesAsync();
            var author = await db.Users.FindAsync(userId);
            return Results.Created($"/api/posts/{id}/comments/{c.Id}", new CommentDto(
                c.Id, c.PostId, c.AuthorId, author!.Username, author.DisplayName, author.AvatarUrl, c.Text, c.CreatedAt));
        }).RequireAuthorization();

        grp.MapDelete("/comments/{commentId:int}", async (int commentId, HttpContext ctx, AppDbContext db) =>
        {
            var userId = ctx.User.RequireUserId();
            var c = await db.Comments.FindAsync(commentId);
            if (c is null) return Results.NotFound();
            if (c.AuthorId != userId) return Results.Forbid();
            db.Comments.Remove(c);
            await db.SaveChangesAsync();
            return Results.NoContent();
        }).RequireAuthorization();

        return app;
    }

    private static PostDto ToDto(Post p, int? currentUserId) => new(
        p.Id, p.AuthorId, p.Author.Username, p.Author.DisplayName, p.Author.AvatarUrl, p.Author.Role,
        p.Title, p.Body,
        p.MediaItems.OrderBy(m => m.SortOrder).Select(m => m.Url).ToList(),
        p.Kind, p.CreatedAt,
        p.Likes.Count, p.Comments.Count,
        currentUserId.HasValue && p.Likes.Any(l => l.UserId == currentUserId.Value));
}
