using System.ComponentModel.DataAnnotations;

namespace BeautyBeast.Api.Dtos;

public record WorkingDayDto(
    int Day,                      // 0 = Sunday … 6 = Saturday
    int StartMinutes,
    int EndMinutes,
    int? BreakStartMinutes,
    int? BreakEndMinutes
);

public record SetScheduleRequest(
    [Required] List<WorkingDayDto> Days
);

public record SlotDto(
    string Time,                  // "HH:mm" in artist-local time
    int StartMinutes              // minutes from midnight
);

public record SlotsResponse(
    string Date,                  // "yyyy-MM-dd"
    int TreatmentId,
    int DurationMinutes,
    List<SlotDto> Slots,
    bool DayOff,                  // true if the artist doesn't work this day
    string? Reason                // optional explanation when no slots
);
