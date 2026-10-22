using BeautyBeast.Api.Data;
using BeautyBeast.Api.Dtos;
using BeautyBeast.Api.Entities;
using BeautyBeast.Api.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BeautyBeast.Api.Endpoints;

public static class ScheduleEndpoints
{
    public static IEndpointRouteBuilder MapScheduleEndpoints(this IEndpointRouteBuilder app)
    {
        // Public read: anyone can see an artist's working hours
        app.MapGet("/api/artists/{artistId:int}/schedule", async (int artistId, AppDbContext db) =>
        {
            var rows = await db.WorkingDays
                .Where(w => w.ArtistId == artistId)
                .OrderBy(w => w.Day)
                .Select(w => new WorkingDayDto((int)w.Day, w.StartMinutes, w.EndMinutes, w.BreakStartMinutes, w.BreakEndMinutes))
                .ToListAsync();
            return Results.Ok(rows);
        }).WithTags("Schedule");

        // My schedule (current user must be an artist)
        app.MapGet("/api/me/schedule", async (HttpContext ctx, AppDbContext db) =>
        {
            var id = ctx.User.RequireUserId();
            var rows = await db.WorkingDays
                .Where(w => w.ArtistId == id)
                .OrderBy(w => w.Day)
                .Select(w => new WorkingDayDto((int)w.Day, w.StartMinutes, w.EndMinutes, w.BreakStartMinutes, w.BreakEndMinutes))
                .ToListAsync();
            return Results.Ok(rows);
        }).RequireAuthorization().WithTags("Schedule");

        // Replace my schedule entirely. Send all days in one request.
        app.MapPut("/api/me/schedule", async (HttpContext ctx, [FromBody] SetScheduleRequest req, AppDbContext db) =>
        {
            var id = ctx.User.RequireUserId();
            var user = await db.Users.FindAsync(id);
            if (user is null || user.Role != UserRole.Artist) return Results.Forbid();

            // Validate
            foreach (var d in req.Days)
            {
                if (d.Day < 0 || d.Day > 6) return Results.BadRequest(new { error = $"Invalid day {d.Day}." });
                if (d.StartMinutes < 0 || d.EndMinutes > 24 * 60 || d.EndMinutes <= d.StartMinutes)
                    return Results.BadRequest(new { error = $"Invalid hours on day {d.Day}." });
                if (d.BreakStartMinutes is int bs && d.BreakEndMinutes is int be)
                {
                    if (be <= bs || bs < d.StartMinutes || be > d.EndMinutes)
                        return Results.BadRequest(new { error = $"Invalid break on day {d.Day}." });
                }
                else if (d.BreakStartMinutes.HasValue != d.BreakEndMinutes.HasValue)
                {
                    return Results.BadRequest(new { error = $"Break on day {d.Day} must have both start and end." });
                }
            }

            var existing = await db.WorkingDays.Where(w => w.ArtistId == id).ToListAsync();
            db.WorkingDays.RemoveRange(existing);

            foreach (var d in req.Days)
            {
                db.WorkingDays.Add(new WorkingDay
                {
                    ArtistId = id,
                    Day = (DayOfWeek)d.Day,
                    StartMinutes = d.StartMinutes,
                    EndMinutes = d.EndMinutes,
                    BreakStartMinutes = d.BreakStartMinutes,
                    BreakEndMinutes = d.BreakEndMinutes
                });
            }
            await db.SaveChangesAsync();
            return Results.NoContent();
        }).RequireAuthorization().WithTags("Schedule");

        // List available slot start times for a specific treatment on a specific date
        app.MapGet("/api/treatments/{id:int}/slots", async (int id, [FromQuery] string date, AppDbContext db) =>
        {
            if (!DateOnly.TryParse(date, out var dateOnly))
                return Results.BadRequest(new { error = "Invalid date. Use YYYY-MM-DD." });

            var t = await db.Treatments.FirstOrDefaultAsync(x => x.Id == id);
            if (t is null || !t.IsActive)
                return Results.NotFound(new { error = "Treatment not found." });

            var day = await db.WorkingDays.FirstOrDefaultAsync(
                w => w.ArtistId == t.ArtistId && w.Day == dateOnly.DayOfWeek);

            if (day is null)
            {
                return Results.Ok(new SlotsResponse(
                    date, id, t.DurationMinutes, new(), DayOff: true,
                    Reason: "Artist doesn't work this day."));
            }

            // Existing bookings for this artist on this date that block their schedule.
            // We treat Pending and Confirmed as "holding" the slot; Cancelled / Declined
            // do not.
            var dateStart = new DateTime(dateOnly.Year, dateOnly.Month, dateOnly.Day, 0, 0, 0, DateTimeKind.Local);
            var dateEnd = dateStart.AddDays(1);
            var dateStartUtc = dateStart.ToUniversalTime();
            var dateEndUtc = dateEnd.ToUniversalTime();

            var blockingStatuses = new[] { BookingStatus.Pending, BookingStatus.Confirmed, BookingStatus.Completed };
            var bookings = await db.Bookings
                .Where(b => b.ArtistId == t.ArtistId
                            && blockingStatuses.Contains(b.Status)
                            && b.ScheduledFor >= dateStartUtc
                            && b.ScheduledFor < dateEndUtc)
                .Select(b => new { b.ScheduledFor, b.DurationMinutes })
                .ToListAsync();

            var blockers = bookings
                .Select(b =>
                {
                    var local = b.ScheduledFor.ToLocalTime();
                    var startMin = local.Hour * 60 + local.Minute;
                    return (Start: startMin, End: startMin + b.DurationMinutes);
                })
                .ToList();

            // If the chosen date is today, refuse slots that have already started.
            int? earliest = null;
            if (dateOnly == DateOnly.FromDateTime(DateTime.Now))
            {
                var now = DateTime.Now;
                earliest = now.Hour * 60 + now.Minute + 15; // 15-min buffer
            }

            var slotMinutes = SlotCalculator.GenerateSlots(day, t.DurationMinutes, blockers, earliest);
            var slots = slotMinutes.Select(m => new SlotDto(SlotCalculator.FormatHHMM(m), m)).ToList();

            return Results.Ok(new SlotsResponse(
                date, id, t.DurationMinutes, slots,
                DayOff: false,
                Reason: slots.Count == 0 ? "All slots taken or none fit the treatment duration." : null));
        }).WithTags("Schedule");

        return app;
    }
}
