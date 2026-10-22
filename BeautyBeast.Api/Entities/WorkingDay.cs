namespace BeautyBeast.Api.Entities;

// One row per (artist, day-of-week) describing the artist's working hours and
// optional break for that day. Times are stored as minutes from midnight in the
// artist's local timezone (we don't try to model timezones explicitly here).
public class WorkingDay
{
    public int Id { get; set; }

    public int ArtistId { get; set; }
    public User Artist { get; set; } = null!;

    public DayOfWeek Day { get; set; }       // 0 = Sunday … 6 = Saturday

    public int StartMinutes { get; set; }    // e.g. 10:00 → 600
    public int EndMinutes { get; set; }      // e.g. 17:00 → 1020

    public int? BreakStartMinutes { get; set; }
    public int? BreakEndMinutes { get; set; }
}
