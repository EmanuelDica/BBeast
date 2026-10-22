using BeautyBeast.Api.Dtos;

namespace BeautyBeast.Api.Endpoints;

public static class UploadEndpoints
{
    // Map allowed extensions to allowed MIME types. Both must agree before we accept the file —
    // this stops a renamed file (e.g. ".exe" disguised as ".png") from sneaking through.
    private static readonly Dictionary<string, string[]> AllowedTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        [".png"]  = new[] { "image/png" },
        [".jpg"]  = new[] { "image/jpeg" },
        [".jpeg"] = new[] { "image/jpeg" },
        [".gif"]  = new[] { "image/gif" },
        [".webp"] = new[] { "image/webp" },
        [".mp4"]  = new[] { "video/mp4" },
        [".webm"] = new[] { "video/webm" },
        [".pdf"]  = new[] { "application/pdf" },
    };

    // Tiny "magic byte" sniff: confirms the first bytes match the expected file format.
    // Catches a non-trivial fraction of mislabeled uploads.
    private static bool MagicBytesMatch(string ext, byte[] head)
    {
        if (head.Length < 4) return false;
        return ext switch
        {
            ".png"  => head.Length >= 8 && head[0] == 0x89 && head[1] == 0x50 && head[2] == 0x4E && head[3] == 0x47,
            ".jpg" or ".jpeg" => head[0] == 0xFF && head[1] == 0xD8 && head[2] == 0xFF,
            ".gif"  => head[0] == 0x47 && head[1] == 0x49 && head[2] == 0x46 && head[3] == 0x38, // GIF8
            ".webp" => head.Length >= 12 && head[0] == 0x52 && head[1] == 0x49 && head[2] == 0x46 && head[3] == 0x46
                       && head[8] == 0x57 && head[9] == 0x45 && head[10] == 0x42 && head[11] == 0x50, // RIFF…WEBP
            ".mp4"  => head.Length >= 8 && head[4] == 0x66 && head[5] == 0x74 && head[6] == 0x79 && head[7] == 0x70, // ftyp
            ".webm" => head[0] == 0x1A && head[1] == 0x45 && head[2] == 0xDF && head[3] == 0xA3, // EBML
            ".pdf"  => head[0] == 0x25 && head[1] == 0x50 && head[2] == 0x44 && head[3] == 0x46, // %PDF
            _ => false
        };
    }

    public static IEndpointRouteBuilder MapUploadEndpoints(this IEndpointRouteBuilder app)
    {
        var grp = app.MapGroup("/api/uploads").WithTags("Uploads").RequireAuthorization();

        grp.MapPost("/", async (HttpRequest request, IWebHostEnvironment env, HttpContext ctx) =>
        {
            if (!request.HasFormContentType)
                return Results.BadRequest(new { error = "Expected multipart/form-data." });

            var form = await request.ReadFormAsync();
            var file = form.Files["file"] ?? form.Files.FirstOrDefault();
            if (file is null || file.Length == 0)
                return Results.BadRequest(new { error = "No file provided." });

            const long maxBytes = 25 * 1024 * 1024; // 25 MB
            if (file.Length > maxBytes)
                return Results.BadRequest(new { error = "File too large (max 25 MB)." });

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedTypes.TryGetValue(ext, out var allowedMimes))
                return Results.BadRequest(new { error = $"File type {ext} not allowed." });

            // Cross-check declared content-type against the extension.
            var contentType = file.ContentType?.Trim().ToLowerInvariant();
            if (string.IsNullOrEmpty(contentType) || !allowedMimes.Contains(contentType))
                return Results.BadRequest(new { error = $"Content-type '{contentType}' doesn't match extension '{ext}'." });

            // Magic-byte check on the first few bytes.
            await using (var probe = file.OpenReadStream())
            {
                var head = new byte[12];
                var read = await probe.ReadAsync(head, 0, head.Length);
                if (read < 4 || !MagicBytesMatch(ext, head))
                    return Results.BadRequest(new { error = "File contents don't match the declared type." });
            }

            var uploadsDir = Path.Combine(env.WebRootPath ?? "wwwroot", "uploads");
            Directory.CreateDirectory(uploadsDir);

            var safeName = $"{Guid.NewGuid():N}{ext}";
            var fullPath = Path.Combine(uploadsDir, safeName);

            await using (var fs = File.Create(fullPath))
            await using (var src = file.OpenReadStream())
            {
                await src.CopyToAsync(fs);
            }

            // Absolute URL so the frontend (different host/port) can render directly.
            var baseUrl = $"{ctx.Request.Scheme}://{ctx.Request.Host}";
            var url = $"{baseUrl}/uploads/{safeName}";
            return Results.Ok(new UploadResponse(url));
        }).DisableAntiforgery(); // Bearer-token auth, not cookies.

        return app;
    }
}
