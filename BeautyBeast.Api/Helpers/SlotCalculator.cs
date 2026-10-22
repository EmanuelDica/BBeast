using BeautyBeast.Api.Entities;

namespace BeautyBeast.Api.Helpers;

public static class SlotCalculator
{
    // Granularity at which we generate candidate slot starts. Smaller = more
    // options, larger = fewer/cleaner options. 30 minutes is a good default.
    public const int SlotIntervalMinutes = 30;

    /// <summary>
    /// Compute valid start times (as minutes-from-midnight, local) for a treatment
    /// of `treatmentDurationMinutes` on the given working day, excluding times
    /// blocked by `existingBookingsLocalMinutes` (each entry is [startMin, endMin)).
    /// </summary>
    public static List<int> GenerateSlots(
        WorkingDay day,
        int treatmentDurationMinutes,
        IEnumerable<(int Start, int End)> existingBookingsLocalMinutes,
        int? earliestStartMinutes = null)
    {
        if (treatmentDurationMinutes <= 0) return new();

        // Build the work blocks: split around the break if there is one.
        var blocks = new List<(int Start, int End)>();
        if (day.BreakStartMinutes is int bs && day.BreakEndMinutes is int be && be > bs
            && bs >= day.StartMinutes && be <= day.EndMinutes)
        {
            if (bs > day.StartMinutes) blocks.Add((day.StartMinutes, bs));
            if (be < day.EndMinutes) blocks.Add((be, day.EndMinutes));
        }
        else
        {
            blocks.Add((day.StartMinutes, day.EndMinutes));
        }

        var existing = existingBookingsLocalMinutes.ToList();
        var slots = new List<int>();

        foreach (var (blockStart, blockEnd) in blocks)
        {
            // Round up to the next slot grid point ≥ blockStart.
            var first = blockStart;
            if (earliestStartMinutes.HasValue && earliestStartMinutes.Value > first)
                first = earliestStartMinutes.Value;

            // Snap to the slot grid.
            var rem = first % SlotIntervalMinutes;
            if (rem != 0) first += SlotIntervalMinutes - rem;

            for (var t = first; t + treatmentDurationMinutes <= blockEnd; t += SlotIntervalMinutes)
            {
                if (Overlaps(t, t + treatmentDurationMinutes, existing)) continue;
                slots.Add(t);
            }
        }

        return slots;
    }

    /// <summary>
    /// True if a candidate booking from [start, end) (in minutes) overlaps with
    /// any existing reservation.
    /// </summary>
    public static bool Overlaps(int start, int end, IEnumerable<(int Start, int End)> reservations)
    {
        foreach (var r in reservations)
            if (start < r.End && end > r.Start) return true;
        return false;
    }

    public static int ToMinutes(DateTime localDateTime)
        => localDateTime.Hour * 60 + localDateTime.Minute;

    public static string FormatHHMM(int minutesFromMidnight)
        => $"{minutesFromMidnight / 60:D2}:{minutesFromMidnight % 60:D2}";
}
