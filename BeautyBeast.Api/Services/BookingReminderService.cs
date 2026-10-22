using BeautyBeast.Api.Data;
using BeautyBeast.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace BeautyBeast.Api.Services;

/// <summary>
/// Background worker that wakes every 10 minutes and sends booking reminder
/// notifications to clients at 5 fixed intervals before each confirmed booking:
///   • 2 weeks before
///   • 1 week before
///   • 2 days before
///   • 1 day before
///   • 5 hours before
///
/// Each (booking × interval) combination is recorded in <see cref="BookingReminderLog"/>
/// so reminders are sent exactly once even if the service restarts.
/// </summary>
public sealed class BookingReminderService : BackgroundService
{
    private static readonly (ReminderInterval Interval, TimeSpan Before)[] Checkpoints =
    [
        (ReminderInterval.TwoWeeks, TimeSpan.FromDays(14)),
        (ReminderInterval.OneWeek,  TimeSpan.FromDays(7)),
        (ReminderInterval.TwoDays,  TimeSpan.FromDays(2)),
        (ReminderInterval.OneDay,   TimeSpan.FromDays(1)),
        (ReminderInterval.FiveHours, TimeSpan.FromHours(5)),
    ];

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<BookingReminderService> _logger;

    public BookingReminderService(IServiceScopeFactory scopeFactory, ILogger<BookingReminderService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Brief startup delay so the API is fully initialised before we query the DB.
        await Task.Delay(TimeSpan.FromSeconds(15), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessRemindersAsync(stoppingToken);
            }
            catch (OperationCanceledException) { break; }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Booking reminder sweep failed");
            }

            await Task.Delay(TimeSpan.FromMinutes(10), stoppingToken)
                      .ContinueWith(_ => { }); // swallow cancellation so we exit cleanly
        }
    }

    private static string BuildMessage(ReminderInterval interval, DateTime scheduledFor, DateTime now, string treatmentName)
    {
        var timeLeft = scheduledFor - now;

        // Human-readable "time left" based on the reminder interval label.
        var timeLeftStr = interval switch
        {
            ReminderInterval.TwoWeeks  => "in 2 weeks",
            ReminderInterval.OneWeek   => "in 1 week",
            ReminderInterval.TwoDays   => "in 2 days",
            ReminderInterval.OneDay    => "tomorrow",
            ReminderInterval.FiveHours => $"in about {(int)Math.Ceiling(timeLeft.TotalHours)} hour{((int)Math.Ceiling(timeLeft.TotalHours) == 1 ? "" : "s")}",
            _                          => $"in {(int)timeLeft.TotalHours}h"
        };

        return $"{treatmentName} — {timeLeftStr} ({scheduledFor:ddd d MMM 'at' HH:mm})";
    }

    private async Task ProcessRemindersAsync(CancellationToken ct)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var now = DateTime.UtcNow;

        // Load confirmed bookings that are still in the future (up to 15 days ahead).
        var cutoff = now.AddDays(15);
        var bookings = await db.Bookings
            .Include(b => b.Treatment)
            .Where(b => b.Status == BookingStatus.Confirmed
                     && b.ScheduledFor > now
                     && b.ScheduledFor <= cutoff)
            .ToListAsync(ct);

        if (bookings.Count == 0) return;

        var bookingIds = bookings.Select(b => b.Id).ToList();

        // Fetch all already-sent logs for these bookings in one query.
        var sentPairs = (await db.BookingReminderLogs
            .Where(l => bookingIds.Contains(l.BookingId))
            .Select(l => new { l.BookingId, l.Interval })
            .ToListAsync(ct))
            .Select(l => (l.BookingId, l.Interval))
            .ToHashSet();

        int dispatched = 0;

        foreach (var booking in bookings)
        {
            foreach (var (interval, before) in Checkpoints)
            {
                // Skip if already sent.
                if (sentPairs.Contains((booking.Id, interval))) continue;

                // Fire when we're within the reminder window.
                var fireAt = booking.ScheduledFor - before;
                if (now < fireAt) continue; // not due yet

                // If the reminder window predates the booking itself, skip it.
                // e.g. booking made today for tomorrow — don't fire the 2-week or 1-week reminders.
                if (fireAt <= booking.CreatedAt) continue;

                // Create the notification for the client.
                db.Notifications.Add(new Notification
                {
                    RecipientId = booking.ClientId,
                    ActorId     = null,   // system-generated
                    Kind        = NotificationKind.BookingReminder,
                    BookingId   = booking.Id,
                    Message     = BuildMessage(interval, booking.ScheduledFor, now, booking.Treatment.Name)
                });

                // Mark as sent to prevent duplicates.
                db.BookingReminderLogs.Add(new BookingReminderLog
                {
                    BookingId = booking.Id,
                    Interval  = interval,
                    SentAt    = now
                });

                sentPairs.Add((booking.Id, interval)); // guard within this sweep
                dispatched++;
            }
        }

        if (dispatched > 0)
        {
            await db.SaveChangesAsync(ct);
            _logger.LogInformation(
                "BookingReminderService dispatched {Count} reminder notification(s)", dispatched);
        }
    }
}
