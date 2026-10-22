using BeautyBeast.Api.Data;
using BeautyBeast.Api.Dtos;
using BeautyBeast.Api.Entities;
using BeautyBeast.Api.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BeautyBeast.Api.Endpoints;

public static class ReviewEndpoints
{
    public static IEndpointRouteBuilder MapReviewEndpoints(this IEndpointRouteBuilder app)
    {
        var grp = app.MapGroup("/api/reviews").WithTags("Reviews");

        // Reviews for an artist
        grp.MapGet("/", async (AppDbContext db, [FromQuery] int artistId) =>
        {
            var list = await db.Reviews
                .Include(r => r.Client)
                .Where(r => r.ArtistId == artistId)
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => new ReviewDto(
                    r.Id, r.ClientId, r.Client.Username, r.Client.DisplayName, r.Client.AvatarUrl,
                    r.ArtistId, r.Rating, r.Text, r.CreatedAt, r.BookingId))
                .ToListAsync();
            return Results.Ok(list);
        });

        // Create a review (clients only). Per outline, "only under specific conditions" — we require
        // a Completed booking with this artist before allowing the review.
        grp.MapPost("/", async (HttpContext ctx, [FromBody] CreateReviewRequest req, AppDbContext db) =>
        {
            var id = ctx.User.RequireUserId();
            var role = ctx.User.UserRoleValue();
            if (role != UserRole.Client) return Results.Forbid();

            var hasCompleted = await db.Bookings.AnyAsync(b =>
                b.ClientId == id && b.ArtistId == req.ArtistId && b.Status == BookingStatus.Completed);
            if (!hasCompleted)
                return Results.BadRequest(new { error = "You can review an artist only after a completed booking." });

            var artist = await db.Users.FindAsync(req.ArtistId);
            if (artist is null || artist.Role != UserRole.Artist) return Results.NotFound();

            var r = new Review
            {
                ClientId = id,
                ArtistId = req.ArtistId,
                BookingId = req.BookingId,
                Rating = Math.Clamp(req.Rating, 1, 5),
                Text = req.Text.Trim()
            };
            db.Reviews.Add(r);
            await db.SaveChangesAsync();
            var client = await db.Users.FindAsync(id);
            return Results.Created($"/api/reviews/{r.Id}", new ReviewDto(
                r.Id, r.ClientId, client!.Username, client.DisplayName, client.AvatarUrl,
                r.ArtistId, r.Rating, r.Text, r.CreatedAt, r.BookingId));
        }).RequireAuthorization();

        grp.MapDelete("/{id:int}", async (int id, HttpContext ctx, AppDbContext db) =>
        {
            var userId = ctx.User.RequireUserId();
            var r = await db.Reviews.FindAsync(id);
            if (r is null) return Results.NotFound();
            if (r.ClientId != userId) return Results.Forbid();
            db.Reviews.Remove(r);
            await db.SaveChangesAsync();
            return Results.NoContent();
        }).RequireAuthorization();

        return app;
    }
}
