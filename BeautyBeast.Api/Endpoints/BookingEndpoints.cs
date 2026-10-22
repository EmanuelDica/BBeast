using BeautyBeast.Api.Data;
using BeautyBeast.Api.Dtos;
using BeautyBeast.Api.Entities;
using BeautyBeast.Api.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BeautyBeast.Api.Endpoints;

public static class BookingEndpoints
{
    public static IEndpointRouteBuilder MapBookingEndpoints(this IEndpointRouteBuilder app)
    {
        var grp = app.MapGroup("/api/bookings").WithTags("Bookings").RequireAuthorization();

        // All bookings the current user is part of — as client, as artist, or both.
        // The UI distinguishes the two by comparing ClientId / ArtistId to the current user.
        grp.MapGet("/me", async (HttpContext ctx, AppDbContext db) =>
        {
            var id = ctx.User.RequireUserId();
            var list = await db.Bookings
                .Include(b => b.Client).Include(b => b.Artist).Include(b => b.Treatment)
                .Where(b => b.ClientId == id || b.ArtistId == id)
                .OrderByDescending(b => b.ScheduledFor)
                .Select(b => Map(b))
                .ToListAsync();
            return Results.Ok(list);
        });

        grp.MapGet("/{id:int}", async (int id, HttpContext ctx, AppDbContext db) =>
        {
            var userId = ctx.User.RequireUserId();
            var b = await db.Bookings.Include(x => x.Client).Include(x => x.Artist).Include(x => x.Treatment)
                .FirstOrDefaultAsync(x => x.Id == id);
            if (b is null) return Results.NotFound();
            if (b.ClientId != userId && b.ArtistId != userId) return Results.Forbid();
            return Results.Ok(Map(b));
        });

        // Create a booking. Any authenticated user can place a booking — including artists
        // booking treatments with another artist. Restrictions:
        //  - Can't book your own treatment.
        //  - Time must be in the future.
        //  - Time must fall on a valid slot per the artist's schedule, with no clash.
        grp.MapPost("/", async (HttpContext ctx, [FromBody] CreateBookingRequest req, AppDbContext db) =>
        {
            var id = ctx.User.RequireUserId();

            var t = await db.Treatments.Include(x => x.Artist).FirstOrDefaultAsync(x => x.Id == req.TreatmentId);
            if (t is null || !t.IsActive) return Results.BadRequest(new { error = "Treatment not available." });
            if (t.ArtistId == id) return Results.BadRequest(new { error = "You can't book your own treatment." });

            var requestedUtc = DateTime.SpecifyKind(req.ScheduledFor, DateTimeKind.Utc);
            if (requestedUtc < DateTime.UtcNow.AddMinutes(15))
                return Results.BadRequest(new { error = "Pick a time in the future." });

            // Validate against the artist's working schedule.
            var local = requestedUtc.ToLocalTime();
            var day = await db.WorkingDays.FirstOrDefaultAsync(
                w => w.ArtistId == t.ArtistId && w.Day == local.DayOfWeek);
            if (day is null)
                return Results.BadRequest(new { error = "The artist doesn't work that day." });

            var startMin = local.Hour * 60 + local.Minute;
            var endMin = startMin + t.DurationMinutes;

            // Must fit inside a working block (account for break).
            bool insideBlock;
            if (day.BreakStartMinutes is int bs && day.BreakEndMinutes is int be)
            {
                insideBlock =
                    (startMin >= day.StartMinutes && endMin <= bs) ||
                    (startMin >= be && endMin <= day.EndMinutes);
            }
            else
            {
                insideBlock = startMin >= day.StartMinutes && endMin <= day.EndMinutes;
            }
            if (!insideBlock)
                return Results.BadRequest(new { error = "That time isn't within the artist's working hours." });

            // Snap to slot grid.
            if (startMin % SlotCalculator.SlotIntervalMinutes != 0)
                return Results.BadRequest(new { error = "Pick one of the offered slots." });

            // Check for overlapping bookings on the same artist + day.
            var dayStartUtc = new DateTime(local.Year, local.Month, local.Day, 0, 0, 0, DateTimeKind.Local).ToUniversalTime();
            var dayEndUtc = dayStartUtc.AddDays(1);
            var blockingStatuses = new[] { BookingStatus.Pending, BookingStatus.Confirmed, BookingStatus.Completed };
            var sameDay = await db.Bookings
                .Where(b => b.ArtistId == t.ArtistId
                            && blockingStatuses.Contains(b.Status)
                            && b.ScheduledFor >= dayStartUtc
                            && b.ScheduledFor < dayEndUtc)
                .Select(b => new { b.ScheduledFor, b.DurationMinutes })
                .ToListAsync();
            foreach (var b in sameDay)
            {
                var bLocal = b.ScheduledFor.ToLocalTime();
                var bStart = bLocal.Hour * 60 + bLocal.Minute;
                var bEnd = bStart + b.DurationMinutes;
                if (startMin < bEnd && endMin > bStart)
                    return Results.Conflict(new { error = "That slot has already been booked." });
            }

            var booking = new Booking
            {
                ClientId = id,
                ArtistId = t.ArtistId,
                TreatmentId = t.Id,
                ScheduledFor = requestedUtc,
                DurationMinutes = t.DurationMinutes,
                Status = BookingStatus.Pending,
                ClientNotes = req.ClientNotes
            };
            db.Bookings.Add(booking);

            await db.SaveChangesAsync(); // gives booking its Id

            // Notify the artist that they have a new booking request.
            db.Notifications.Add(new Notification
            {
                RecipientId = t.ArtistId,
                ActorId     = id,
                Kind        = NotificationKind.BookingRequested,
                BookingId   = booking.Id
            });
            await db.SaveChangesAsync();

            await db.Entry(booking).Reference(x => x.Client).LoadAsync();
            await db.Entry(booking).Reference(x => x.Artist).LoadAsync();
            await db.Entry(booking).Reference(x => x.Treatment).LoadAsync();
            return Results.Created($"/api/bookings/{booking.Id}", Map(booking));
        });

        // Update booking status: artist confirms/declines/completes; client cancels.
        grp.MapPut("/{id:int}/status", async (int id, HttpContext ctx, [FromBody] UpdateBookingStatusRequest req, AppDbContext db) =>
        {
            var userId = ctx.User.RequireUserId();
            var b = await db.Bookings.FindAsync(id);
            if (b is null) return Results.NotFound();
            if (b.ClientId != userId && b.ArtistId != userId) return Results.Forbid();

            // Permission rules
            NotificationKind? notifKind = null;
            int? notifRecipient = null;

            if (b.ArtistId == userId)
            {
                // Artist can do anything
                b.Status = req.Status;
                b.ArtistNotes = req.ArtistNotes ?? b.ArtistNotes;

                // Tell the client when confirmed or cancelled.
                if (req.Status == BookingStatus.Confirmed)
                {
                    notifKind = NotificationKind.BookingConfirmed;
                    notifRecipient = b.ClientId;
                }
                else if (req.Status == BookingStatus.Cancelled)
                {
                    notifKind = NotificationKind.BookingCancelled;
                    notifRecipient = b.ClientId;
                }
            }
            else if (b.ClientId == userId)
            {
                // Client may only cancel
                if (req.Status != BookingStatus.Cancelled)
                    return Results.Forbid();
                b.Status = BookingStatus.Cancelled;

                // Tell the artist their booking was cancelled.
                notifKind = NotificationKind.BookingCancelled;
                notifRecipient = b.ArtistId;
            }

            b.UpdatedAt = DateTime.UtcNow;

            if (notifKind is not null && notifRecipient is not null)
            {
                db.Notifications.Add(new Notification
                {
                    RecipientId = notifRecipient.Value,
                    ActorId     = userId,
                    Kind        = notifKind.Value,
                    BookingId   = b.Id
                });
            }

            await db.SaveChangesAsync();
            return Results.NoContent();
        });

        return app;
    }

    private static BookingDto Map(Booking b) => new(
        b.Id,
        b.ClientId, b.Client.Username, b.Client.DisplayName, b.Client.AvatarUrl,
        b.ArtistId, b.Artist.Username, b.Artist.DisplayName, b.Artist.AvatarUrl,
        b.TreatmentId, b.Treatment.Name, b.Treatment.Price,
        b.ScheduledFor, b.DurationMinutes, b.Status, b.ClientNotes, b.ArtistNotes, b.CreatedAt);
}
