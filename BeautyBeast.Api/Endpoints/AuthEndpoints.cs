using BeautyBeast.Api.Data;
using BeautyBeast.Api.Dtos;
using BeautyBeast.Api.Entities;
using BeautyBeast.Api.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

namespace BeautyBeast.Api.Endpoints;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        // Apply the "auth" rate-limit policy to the whole group so /register
        // and /login can't be brute-forced.
        var grp = app.MapGroup("/api/auth").WithTags("Auth").RequireRateLimiting("auth");

        grp.MapPost("/register", async (
            [FromBody] RegisterRequest req,
            AppDbContext db,
            JwtHelper jwt) =>
        {
            if (string.IsNullOrWhiteSpace(req.Username) || string.IsNullOrWhiteSpace(req.Password))
                return Results.BadRequest(new { error = "Username and password are required." });

            // Public registration may only create Client or Artist accounts.
            // Admin accounts are provisioned out-of-band (seed data / a future admin tool),
            // never via this endpoint — otherwise anyone can self-assign Admin.
            if (req.Role != UserRole.Client && req.Role != UserRole.Artist)
                return Results.BadRequest(new { error = "Invalid role." });

            var username = req.Username.Trim().ToLowerInvariant();
            var email = req.Email.Trim().ToLowerInvariant();

            if (await db.Users.AnyAsync(u => u.Username == username))
                return Results.Conflict(new { error = "Username already taken." });
            if (await db.Users.AnyAsync(u => u.Email == email))
                return Results.Conflict(new { error = "Email already registered." });

            var user = new User
            {
                Username = username,
                Email = email,
                DisplayName = req.DisplayName.Trim(),
                PasswordHash = PasswordHelper.Hash(req.Password),
                Role = req.Role
            };

            // Auto-create the matching profile
            if (req.Role == UserRole.Artist)
                user.ArtistProfile = new ArtistProfile();
            else if (req.Role == UserRole.Client)
                user.ClientProfile = new ClientProfile();

            db.Users.Add(user);
            await db.SaveChangesAsync();

            var (token, expiresAt) = jwt.Issue(user);
            return Results.Ok(new AuthResponse(token, expiresAt, user.Id, user.Username, user.DisplayName, user.Role, user.AvatarUrl));
        });

        grp.MapPost("/login", async (
            [FromBody] LoginRequest req,
            AppDbContext db,
            JwtHelper jwt) =>
        {
            var key = (req.UsernameOrEmail ?? string.Empty).Trim().ToLowerInvariant();
            var user = await db.Users
                .FirstOrDefaultAsync(u => u.Username == key || u.Email == key);

            if (user is null || !PasswordHelper.Verify(req.Password, user.PasswordHash))
                return Results.Unauthorized();

            var (token, expiresAt) = jwt.Issue(user);
            return Results.Ok(new AuthResponse(token, expiresAt, user.Id, user.Username, user.DisplayName, user.Role, user.AvatarUrl));
        });

        grp.MapGet("/me", async (HttpContext ctx, AppDbContext db) =>
        {
            var id = ctx.User.UserId();
            if (id is null) return Results.Unauthorized();
            var user = await db.Users.FindAsync(id.Value);
            if (user is null) return Results.NotFound();
            return Results.Ok(new CurrentUserDto(
                user.Id, user.Username, user.Email, user.DisplayName, user.Role,
                user.Bio, user.AvatarUrl, user.CoverUrl));
        }).RequireAuthorization();

        return app;
    }
}
