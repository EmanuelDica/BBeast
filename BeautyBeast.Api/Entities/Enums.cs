namespace BeautyBeast.Api.Entities;

public enum UserRole
{
    Client = 0,
    Artist = 1,
    Admin = 2
}

public enum BookingStatus
{
    Pending = 0,
    Confirmed = 1,
    Cancelled = 2,
    Completed = 3,
    Declined = 4
}

public enum PostKind
{
    Text = 0,
    Image = 1,
    Video = 2,
    Article = 3
}

public enum ResourceKind
{
    Article = 0,
    Video = 1,
    Document = 2,
    Link = 3
}

public enum NotificationKind
{
    PostLiked = 0,
    PostCommented = 1,
    StatusLiked = 2,
    StatusCommented = 3,
    BookingRequested = 4,
    BookingConfirmed = 5,
    BookingCancelled = 6,
    MessageReceived = 7,
    BookingReminder = 8
}

public enum ReminderInterval
{
    TwoWeeks = 0,
    OneWeek = 1,
    TwoDays = 2,
    OneDay = 3,
    FiveHours = 4
}
