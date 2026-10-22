using BeautyBeast.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace BeautyBeast.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<ArtistProfile> ArtistProfiles => Set<ArtistProfile>();
    public DbSet<ClientProfile> ClientProfiles => Set<ClientProfile>();
    public DbSet<Post> Posts => Set<Post>();
    public DbSet<PostMedia> PostMediaItems => Set<PostMedia>();
    public DbSet<StatusUpdate> Statuses => Set<StatusUpdate>();
    public DbSet<Comment> Comments => Set<Comment>();
    public DbSet<PostLike> PostLikes => Set<PostLike>();
    public DbSet<Treatment> Treatments => Set<Treatment>();
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<Resource> Resources => Set<Resource>();
    public DbSet<ConsentForm> ConsentForms => Set<ConsentForm>();
    public DbSet<Achievement> Achievements => Set<Achievement>();
    public DbSet<WorkingDay> WorkingDays => Set<WorkingDay>();
    public DbSet<StatusLike> StatusLikes => Set<StatusLike>();
    public DbSet<StatusComment> StatusComments => Set<StatusComment>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<BookingReminderLog> BookingReminderLogs => Set<BookingReminderLog>();
    public DbSet<Conversation> Conversations => Set<Conversation>();
    public DbSet<DirectMessage> DirectMessages => Set<DirectMessage>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);

        b.Entity<User>(e =>
        {
            e.HasIndex(u => u.Username).IsUnique();
            e.HasIndex(u => u.Email).IsUnique();

            e.HasOne(u => u.ArtistProfile)
             .WithOne(p => p.User)
             .HasForeignKey<ArtistProfile>(p => p.UserId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(u => u.ClientProfile)
             .WithOne(p => p.User)
             .HasForeignKey<ClientProfile>(p => p.UserId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        b.Entity<Post>(e =>
        {
            e.HasOne(p => p.Author)
             .WithMany(u => u.Posts)
             .HasForeignKey(p => p.AuthorId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        b.Entity<PostMedia>(e =>
        {
            e.HasOne(m => m.Post)
             .WithMany(p => p.MediaItems)
             .HasForeignKey(m => m.PostId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        b.Entity<StatusUpdate>(e =>
        {
            e.HasOne(s => s.Author)
             .WithMany(u => u.Statuses)
             .HasForeignKey(s => s.AuthorId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        b.Entity<Comment>(e =>
        {
            e.HasOne(c => c.Post)
             .WithMany(p => p.Comments)
             .HasForeignKey(c => c.PostId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(c => c.Author)
             .WithMany(u => u.Comments)
             .HasForeignKey(c => c.AuthorId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        b.Entity<PostLike>(e =>
        {
            e.HasIndex(l => new { l.PostId, l.UserId }).IsUnique();

            e.HasOne(l => l.Post)
             .WithMany(p => p.Likes)
             .HasForeignKey(l => l.PostId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(l => l.User)
             .WithMany(u => u.Likes)
             .HasForeignKey(l => l.UserId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        b.Entity<Treatment>(e =>
        {
            e.Property(t => t.Price).HasColumnType("decimal(10,2)");

            e.HasOne(t => t.Artist)
             .WithMany(u => u.Treatments)
             .HasForeignKey(t => t.ArtistId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        b.Entity<Booking>(e =>
        {
            e.HasOne(bk => bk.Client)
             .WithMany(u => u.BookingsAsClient)
             .HasForeignKey(bk => bk.ClientId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(bk => bk.Artist)
             .WithMany(u => u.BookingsAsArtist)
             .HasForeignKey(bk => bk.ArtistId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(bk => bk.Treatment)
             .WithMany(t => t.Bookings)
             .HasForeignKey(bk => bk.TreatmentId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        b.Entity<Review>(e =>
        {
            e.HasOne(r => r.Client)
             .WithMany(u => u.ReviewsWritten)
             .HasForeignKey(r => r.ClientId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(r => r.Artist)
             .WithMany(u => u.ReviewsReceived)
             .HasForeignKey(r => r.ArtistId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(r => r.Booking)
             .WithMany()
             .HasForeignKey(r => r.BookingId)
             .OnDelete(DeleteBehavior.SetNull);
        });

        b.Entity<Resource>(e =>
        {
            e.HasOne(r => r.Author)
             .WithMany(u => u.ResourcesAuthored)
             .HasForeignKey(r => r.AuthorId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        b.Entity<ConsentForm>(e =>
        {
            e.HasOne(c => c.Owner)
             .WithMany()
             .HasForeignKey(c => c.OwnerId)
             .OnDelete(DeleteBehavior.SetNull);
        });

        b.Entity<Achievement>(e =>
        {
            e.HasOne(a => a.User)
             .WithMany(u => u.Achievements)
             .HasForeignKey(a => a.UserId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        b.Entity<WorkingDay>(e =>
        {
            e.HasIndex(w => new { w.ArtistId, w.Day }).IsUnique();
            e.HasOne(w => w.Artist)
             .WithMany(u => u.WorkingDays)
             .HasForeignKey(w => w.ArtistId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        b.Entity<StatusLike>(e =>
        {
            e.HasIndex(l => new { l.StatusId, l.UserId }).IsUnique();

            e.HasOne(l => l.Status)
             .WithMany(s => s.Likes)
             .HasForeignKey(l => l.StatusId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(l => l.User)
             .WithMany(u => u.StatusLikes)
             .HasForeignKey(l => l.UserId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        b.Entity<StatusComment>(e =>
        {
            e.HasOne(c => c.Status)
             .WithMany(s => s.Comments)
             .HasForeignKey(c => c.StatusId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(c => c.Author)
             .WithMany(u => u.StatusComments)
             .HasForeignKey(c => c.AuthorId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        b.Entity<Notification>(e =>
        {
            e.HasOne(n => n.Recipient)
             .WithMany(u => u.Notifications)
             .HasForeignKey(n => n.RecipientId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(n => n.Actor)
             .WithMany()
             .HasForeignKey(n => n.ActorId)
             .OnDelete(DeleteBehavior.SetNull);
        });

        b.Entity<Conversation>(e =>
        {
            // Unique pair — prevents duplicate threads between the same two users.
            e.HasIndex(c => new { c.ParticipantAId, c.ParticipantBId }).IsUnique();

            e.HasOne(c => c.ParticipantA)
             .WithMany()
             .HasForeignKey(c => c.ParticipantAId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(c => c.ParticipantB)
             .WithMany()
             .HasForeignKey(c => c.ParticipantBId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        b.Entity<DirectMessage>(e =>
        {
            e.HasOne(m => m.Conversation)
             .WithMany(c => c.Messages)
             .HasForeignKey(m => m.ConversationId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(m => m.Sender)
             .WithMany()
             .HasForeignKey(m => m.SenderId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        b.Entity<BookingReminderLog>(e =>
        {
            // One record per (booking × interval) — guarantees no duplicate reminders.
            e.HasIndex(r => new { r.BookingId, r.Interval }).IsUnique();

            e.HasOne(r => r.Booking)
             .WithMany()
             .HasForeignKey(r => r.BookingId)
             .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
