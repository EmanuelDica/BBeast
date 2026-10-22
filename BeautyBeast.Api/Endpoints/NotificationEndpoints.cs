using BeautyBeast.Api.Data;
using BeautyBeast.Api.Dtos;
using BeautyBeast.Api.Helpers;
using Microsoft.EntityFrameworkCore;

namespace BeautyBeast.Api.Endpoints;

public static class NotificationEndpoints
{
    public static IEndpointRouteBuilder MapNotificationEndpoints(this IEndpointRouteBuilder app)
    {
        var grp = app.MapGroup("/api/notifications").WithTags("Notifications").RequireAuthorization();

        // GET /api/notifications — returns the 50 most recent for the current user
        grp.MapGet("/", async (HttpContext ctx, AppDbContext db) =>
        {
            var userId = ctx.User.RequireUserId();
            var list = await db.Notifications
                .Where(n => n.RecipientId == userId)
                .Include(n => n.Actor)
                .OrderByDescending(n => n.CreatedAt)
                .Take(50)
                .Select(n => new NotificationDto(
                    n.Id, n.RecipientId,
                    n.ActorId, n.Actor != null ? n.Actor.Username : null,
                    n.Actor != null ? n.Actor.DisplayName : null,
                    n.Actor != null ? n.Actor.AvatarUrl : null,
                    n.Kind, n.PostId, n.StatusId, n.BookingId,
                    n.Message, n.IsRead, n.CreatedAt))
                .ToListAsync();
            return Results.Ok(list);
        });

        // GET /api/notifications/unread-count
        grp.MapGet("/unread-count", async (HttpContext ctx, AppDbContext db) =>
        {
            var userId = ctx.User.RequireUserId();
            var count = await db.Notifications.CountAsync(n => n.RecipientId == userId && !n.IsRead);
            return Results.Ok(new { count });
        });

        // PUT /api/notifications/read-all
        grp.MapPut("/read-all", async (HttpContext ctx, AppDbContext db) =>
        {
            var userId = ctx.User.RequireUserId();
            await db.Notifications
                .Where(n => n.RecipientId == userId && !n.IsRead)
                .ExecuteUpdateAsync(s => s.SetProperty(n => n.IsRead, true));
            return Results.NoContent();
        });

        // PUT /api/notifications/{id}/read
        grp.MapPut("/{id:int}/read", async (int id, HttpContext ctx, AppDbContext db) =>
        {
            var userId = ctx.User.RequireUserId();
            var n = await db.Notifications.FindAsync(id);
            if (n is null) return Results.NotFound();
            if (n.RecipientId != userId) return Results.Forbid();
            n.IsRead = true;
            await db.SaveChangesAsync();
            return Results.NoContent();
        });

        return app;
    }
}
