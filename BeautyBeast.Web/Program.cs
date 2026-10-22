using BeautyBeast.Web.Auth;
using BeautyBeast.Web.Components;
using BeautyBeast.Web.Services;
using Microsoft.AspNetCore.Components.Authorization;

var builder = WebApplication.CreateBuilder(args);

// Razor Components / Blazor Server
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// API base URL
var apiBaseUrl = builder.Configuration["Api:BaseUrl"]
    ?? throw new InvalidOperationException("Api:BaseUrl missing in configuration.");

// Per-circuit auth + token store
builder.Services.AddScoped<TokenStore>();
builder.Services.AddScoped<JwtAuthStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<JwtAuthStateProvider>());
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddAuthorizationCore();

// Per-circuit HttpClient pointing at the API
builder.Services.AddScoped(sp =>
{
    return new HttpClient { BaseAddress = new Uri(apiBaseUrl) };
});

builder.Services.AddScoped<ApiClient>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<ProfileService>();
builder.Services.AddScoped<PostService>();
builder.Services.AddScoped<StatusService>();
builder.Services.AddScoped<TreatmentService>();
builder.Services.AddScoped<BookingService>();
builder.Services.AddScoped<ReviewService>();
builder.Services.AddScoped<ResourceService>();
builder.Services.AddScoped<ConsentService>();
builder.Services.AddScoped<SearchService>();
builder.Services.AddScoped<UploadService>();
builder.Services.AddScoped<ScheduleService>();
builder.Services.AddScoped<NotificationService>();
builder.Services.AddScoped<MessagesService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStaticFiles();

// Security headers. We deliberately don't add a strict CSP here because Blazor
// Server's SignalR client requires `unsafe-eval`; tightening CSP for Blazor takes
// a careful per-app policy. The headers below are the cheap, safe wins.
app.Use(async (ctx, next) =>
{
    var h = ctx.Response.Headers;
    h["X-Content-Type-Options"] = "nosniff";
    h["X-Frame-Options"] = "DENY";
    h["Referrer-Policy"] = "strict-origin-when-cross-origin";
    h["X-Permitted-Cross-Domain-Policies"] = "none";
    await next();
});

app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
