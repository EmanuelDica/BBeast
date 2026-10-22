using System.Text;
using System.Threading.RateLimiting;
using BeautyBeast.Api.Data;
using BeautyBeast.Api.Endpoints;
using BeautyBeast.Api.Helpers;
using BeautyBeast.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

// Tell Npgsql to treat DateTime.Kind=Unspecified as UTC (matches SQLite behaviour).
// Must be set before any database calls.
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

// --- DB ---
var connectionString = builder.Configuration.GetConnectionString("BeautyBeast")
    ?? throw new InvalidOperationException("Connection string 'BeautyBeast' is not configured.");
builder.Services.AddDbContext<AppDbContext>(o => o.UseNpgsql(connectionString));

// --- JWT ---
// Key must come from configuration (env var Jwt__Key, user-secrets, or appsettings.Development.json).
// Production startup fails fast if it's missing or too short.
var jwtKey = builder.Configuration["Jwt:Key"];
if (string.IsNullOrWhiteSpace(jwtKey))
    throw new InvalidOperationException(
        "Jwt:Key is not configured. Set environment variable 'Jwt__Key' or use user-secrets / appsettings.Development.json.");
if (Encoding.UTF8.GetByteCount(jwtKey) < 32)
    throw new InvalidOperationException("Jwt:Key must be at least 32 bytes for HMAC-SHA256.");
if (!builder.Environment.IsDevelopment() && jwtKey.Contains("dev-only", StringComparison.OrdinalIgnoreCase))
    throw new InvalidOperationException("Refusing to start in non-Development with the dev JWT key. Rotate it.");

var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "BeautyBeast.Api";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "BeautyBeast.Web";

// Background services
builder.Services.AddHostedService<BookingReminderService>();

builder.Services.AddSingleton<JwtHelper>();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });
builder.Services.AddAuthorization();

// --- Rate limiting ---
// "auth" policy: protects /api/auth/* from brute-force attempts.
// 10 attempts per minute per remote IP, then 429 until the window resets.
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("auth", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                AutoReplenishment = true
            }));
    // A gentler global limiter as belt-and-braces.
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 300,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
                AutoReplenishment = true
            }));
});

// --- CORS ---
var origins = builder.Configuration.GetSection("Cors:Origins").Get<string[]>()
    ?? new[] { "http://localhost:5100", "https://localhost:5101" };
builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
    p.WithOrigins(origins)
     .AllowAnyHeader()
     .AllowAnyMethod()
     .AllowCredentials()));

// --- Swagger / OpenAPI ---
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// --- JSON ---
builder.Services.ConfigureHttpJsonOptions(o =>
{
    o.SerializerOptions.PropertyNameCaseInsensitive = true;
    o.SerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
});

var app = builder.Build();

// --- Migrate + seed ---
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
    await DbSeeder.SeedAsync(db);
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
app.UseRateLimiter();        // applies global limiter; per-endpoint policies opt in
app.UseStaticFiles();        // serves /uploads/...

// Security headers — minimal but worthwhile.
app.Use(async (ctx, next) =>
{
    var h = ctx.Response.Headers;
    h["X-Content-Type-Options"] = "nosniff";
    h["X-Frame-Options"] = "DENY";
    h["Referrer-Policy"] = "strict-origin-when-cross-origin";
    h["X-Permitted-Cross-Domain-Policies"] = "none";
    await next();
});

app.UseAuthentication();
app.UseAuthorization();

// Reject tokens whose user no longer exists in the database.
// Runs only on authenticated requests, so public endpoints are unaffected.
app.Use(async (ctx, next) =>
{
    if (ctx.User.Identity?.IsAuthenticated == true)
    {
        var userId = ctx.User.UserId();
        if (userId is null)
        {
            ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return;
        }
        var db = ctx.RequestServices.GetRequiredService<AppDbContext>();
        if (!await db.Users.AnyAsync(u => u.Id == userId.Value))
        {
            ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return;
        }
    }
    await next();
});

app.MapGet("/", () => Results.Ok(new { name = "BeautyBeast.Api", status = "ok", version = "1.0" }));

app.MapAuthEndpoints();
app.MapProfileEndpoints();
app.MapPostEndpoints();
app.MapStatusEndpoints();
app.MapTreatmentEndpoints();
app.MapBookingEndpoints();
app.MapReviewEndpoints();
app.MapResourceEndpoints();
app.MapConsentEndpoints();
app.MapSearchEndpoints();
app.MapUploadEndpoints();
app.MapScheduleEndpoints();
app.MapNotificationEndpoints();
app.MapDirectMessageEndpoints();

app.Run();
