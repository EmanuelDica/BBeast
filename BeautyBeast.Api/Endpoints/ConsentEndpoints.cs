using BeautyBeast.Api.Data;
using BeautyBeast.Api.Dtos;
using BeautyBeast.Api.Entities;
using BeautyBeast.Api.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BeautyBeast.Api.Endpoints;

public static class ConsentEndpoints
{
    public static IEndpointRouteBuilder MapConsentEndpoints(this IEndpointRouteBuilder app)
    {
        var grp = app.MapGroup("/api/consent-forms").WithTags("ConsentForms");

        // Public templates + (if signed in) the artist's own forms.
        grp.MapGet("/", async (HttpContext ctx, AppDbContext db, [FromQuery] string? category) =>
        {
            var userId = ctx.User.UserId();
            var q = db.ConsentForms.Include(c => c.Owner).AsQueryable();
            // Show templates always; show user's own forms when signed in.
            q = q.Where(c => c.OwnerId == null || (userId.HasValue && c.OwnerId == userId.Value));
            if (!string.IsNullOrWhiteSpace(category)) q = q.Where(c => c.Category == category);
            var list = await q.OrderBy(c => c.OwnerId == null ? 0 : 1).ThenBy(c => c.Title)
                .Select(c => new ConsentFormDto(
                    c.Id, c.OwnerId, c.Owner != null ? c.Owner.Username : null,
                    c.Title, c.Category, c.Body, c.IsTemplate, c.UpdatedAt))
                .ToListAsync();
            return Results.Ok(list);
        });

        grp.MapGet("/{id:int}", async (int id, HttpContext ctx, AppDbContext db) =>
        {
            var userId = ctx.User.UserId();
            var c = await db.ConsentForms.Include(x => x.Owner).FirstOrDefaultAsync(x => x.Id == id);
            if (c is null) return Results.NotFound();
            if (c.OwnerId.HasValue && c.OwnerId != userId) return Results.Forbid();
            return Results.Ok(new ConsentFormDto(
                c.Id, c.OwnerId, c.Owner?.Username, c.Title, c.Category, c.Body, c.IsTemplate, c.UpdatedAt));
        });

        grp.MapPost("/", async (HttpContext ctx, [FromBody] CreateConsentFormRequest req, AppDbContext db) =>
        {
            var id = ctx.User.RequireUserId();
            var user = await db.Users.FindAsync(id);
            if (user is null || user.Role != UserRole.Artist) return Results.Forbid();
            var c = new ConsentForm
            {
                OwnerId = id,
                Title = req.Title.Trim(),
                Category = req.Category,
                Body = req.Body,
                IsTemplate = req.IsTemplate
            };
            db.ConsentForms.Add(c);
            await db.SaveChangesAsync();
            return Results.Created($"/api/consent-forms/{c.Id}", new ConsentFormDto(
                c.Id, c.OwnerId, user.Username, c.Title, c.Category, c.Body, c.IsTemplate, c.UpdatedAt));
        }).RequireAuthorization();

        grp.MapPut("/{id:int}", async (int id, HttpContext ctx, [FromBody] CreateConsentFormRequest req, AppDbContext db) =>
        {
            var userId = ctx.User.RequireUserId();
            var c = await db.ConsentForms.FindAsync(id);
            if (c is null) return Results.NotFound();
            if (c.OwnerId != userId) return Results.Forbid();
            c.Title = req.Title.Trim();
            c.Category = req.Category;
            c.Body = req.Body;
            c.IsTemplate = req.IsTemplate;
            c.UpdatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync();
            return Results.NoContent();
        }).RequireAuthorization();

        grp.MapDelete("/{id:int}", async (int id, HttpContext ctx, AppDbContext db) =>
        {
            var userId = ctx.User.RequireUserId();
            var c = await db.ConsentForms.FindAsync(id);
            if (c is null) return Results.NotFound();
            if (c.OwnerId != userId) return Results.Forbid();
            db.ConsentForms.Remove(c);
            await db.SaveChangesAsync();
            return Results.NoContent();
        }).RequireAuthorization();

        return app;
    }
}
