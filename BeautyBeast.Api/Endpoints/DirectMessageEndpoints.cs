using BeautyBeast.Api.Data;
using BeautyBeast.Api.Dtos;
using BeautyBeast.Api.Entities;
using BeautyBeast.Api.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BeautyBeast.Api.Endpoints;

public static class DirectMessageEndpoints
{
    public static IEndpointRouteBuilder MapDirectMessageEndpoints(this IEndpointRouteBuilder app)
    {
        var grp = app.MapGroup("/api/conversations").WithTags("DirectMessages").RequireAuthorization();

        // ── List all conversations for the current user ────────────────────────
        grp.MapGet("/", async (HttpContext ctx, AppDbContext db) =>
        {
            var userId = ctx.User.RequireUserId();

            var conversations = await db.Conversations
                .Where(c => c.ParticipantAId == userId || c.ParticipantBId == userId)
                .Include(c => c.ParticipantA)
                .Include(c => c.ParticipantB)
                .OrderByDescending(c => c.LastMessageAt)
                .ToListAsync();

            var results = new List<ConversationDto>();
            foreach (var c in conversations)
            {
                var other = c.ParticipantAId == userId ? c.ParticipantB : c.ParticipantA;

                var lastMsg = await db.DirectMessages
                    .Where(m => m.ConversationId == c.Id)
                    .OrderByDescending(m => m.CreatedAt)
                    .Select(m => m.Text)
                    .FirstOrDefaultAsync();

                var unread = await db.DirectMessages
                    .CountAsync(m => m.ConversationId == c.Id
                                  && m.SenderId != userId
                                  && !m.IsRead);

                results.Add(new ConversationDto(
                    c.Id, other.Id, other.Username, other.DisplayName, other.AvatarUrl,
                    lastMsg is null ? null : (lastMsg.Length > 60 ? lastMsg[..60] + "…" : lastMsg),
                    c.LastMessageAt, unread));
            }

            return Results.Ok(results);
        });

        // ── Get or create a conversation with a specific user ──────────────────
        grp.MapPost("/with/{otherUserId:int}", async (int otherUserId, HttpContext ctx, AppDbContext db) =>
        {
            var userId = ctx.User.RequireUserId();
            if (userId == otherUserId)
                return Results.BadRequest(new { error = "Cannot message yourself." });

            var other = await db.Users.FindAsync(otherUserId);
            if (other is null) return Results.NotFound();

            // Normalise so A < B — prevents duplicate threads.
            var aId = Math.Min(userId, otherUserId);
            var bId = Math.Max(userId, otherUserId);

            var convo = await db.Conversations
                .Include(c => c.ParticipantA)
                .Include(c => c.ParticipantB)
                .FirstOrDefaultAsync(c => c.ParticipantAId == aId && c.ParticipantBId == bId);

            if (convo is null)
            {
                convo = new Conversation { ParticipantAId = aId, ParticipantBId = bId };
                db.Conversations.Add(convo);
                await db.SaveChangesAsync();
                await db.Entry(convo).Reference(c => c.ParticipantA).LoadAsync();
                await db.Entry(convo).Reference(c => c.ParticipantB).LoadAsync();
            }

            var otherUser = convo.ParticipantAId == userId ? convo.ParticipantB : convo.ParticipantA;
            return Results.Ok(new ConversationDto(
                convo.Id, otherUser.Id, otherUser.Username, otherUser.DisplayName, otherUser.AvatarUrl,
                null, convo.LastMessageAt, 0));
        });

        // ── Get messages in a conversation ─────────────────────────────────────
        grp.MapGet("/{id:int}/messages", async (int id, HttpContext ctx, AppDbContext db,
            [FromQuery] int? before, [FromQuery] int take = 50) =>
        {
            var userId = ctx.User.RequireUserId();
            var convo = await db.Conversations.FindAsync(id);
            if (convo is null) return Results.NotFound();
            if (convo.ParticipantAId != userId && convo.ParticipantBId != userId)
                return Results.Forbid();

            var q = db.DirectMessages
                .Where(m => m.ConversationId == id)
                .Include(m => m.Sender)
                .AsQueryable();

            if (before.HasValue)
                q = q.Where(m => m.Id < before.Value);

            var messages = await q
                .OrderByDescending(m => m.CreatedAt)
                .Take(Math.Min(take, 100))
                .OrderBy(m => m.CreatedAt)
                .Select(m => new DirectMessageDto(
                    m.Id, m.ConversationId, m.SenderId,
                    m.Sender.Username, m.Sender.DisplayName, m.Sender.AvatarUrl,
                    m.Text, m.IsRead, m.CreatedAt))
                .ToListAsync();

            return Results.Ok(messages);
        });

        // ── Send a message ─────────────────────────────────────────────────────
        grp.MapPost("/{id:int}/messages", async (int id, HttpContext ctx,
            [FromBody] SendMessageRequest req, AppDbContext db) =>
        {
            var userId = ctx.User.RequireUserId();
            var convo = await db.Conversations.FindAsync(id);
            if (convo is null) return Results.NotFound();
            if (convo.ParticipantAId != userId && convo.ParticipantBId != userId)
                return Results.Forbid();

            var recipientId = convo.ParticipantAId == userId
                ? convo.ParticipantBId
                : convo.ParticipantAId;

            var msg = new DirectMessage
            {
                ConversationId = id,
                SenderId = userId,
                Text = req.Text.Trim()
            };
            db.DirectMessages.Add(msg);

            convo.LastMessageAt = DateTime.UtcNow;

            // Persistent notification for the recipient.
            var preview = req.Text.Length > 60 ? req.Text[..60] + "…" : req.Text;
            db.Notifications.Add(new Notification
            {
                RecipientId = recipientId,
                ActorId     = userId,
                Kind        = NotificationKind.MessageReceived,
                Message     = preview
            });

            await db.SaveChangesAsync();

            var sender = await db.Users.FindAsync(userId);
            return Results.Created($"/api/conversations/{id}/messages/{msg.Id}",
                new DirectMessageDto(msg.Id, msg.ConversationId, msg.SenderId,
                    sender!.Username, sender.DisplayName, sender.AvatarUrl,
                    msg.Text, msg.IsRead, msg.CreatedAt));
        });

        // ── Mark all messages in a conversation as read ────────────────────────
        grp.MapPut("/{id:int}/read", async (int id, HttpContext ctx, AppDbContext db) =>
        {
            var userId = ctx.User.RequireUserId();
            var convo = await db.Conversations.FindAsync(id);
            if (convo is null) return Results.NotFound();
            if (convo.ParticipantAId != userId && convo.ParticipantBId != userId)
                return Results.Forbid();

            await db.DirectMessages
                .Where(m => m.ConversationId == id && m.SenderId != userId && !m.IsRead)
                .ExecuteUpdateAsync(s => s.SetProperty(m => m.IsRead, true));

            return Results.NoContent();
        });

        return app;
    }
}
