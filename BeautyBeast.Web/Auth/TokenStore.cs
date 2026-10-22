using BeautyBeast.Web.Models;

namespace BeautyBeast.Web.Auth;

// Per-circuit in-memory cache of the current user + token.
// Persistence across reloads is handled by SessionAuthInitializer + ProtectedSessionStorage.
public class TokenStore
{
    public string? Token { get; private set; }
    public DateTime? ExpiresAt { get; private set; }
    public AuthUser? User { get; private set; }

    public bool IsAuthenticated => !string.IsNullOrWhiteSpace(Token)
        && (ExpiresAt is null || ExpiresAt > DateTime.UtcNow.AddSeconds(5));

    public event Action? Changed;
    // Fired when the API returns 401 — subscribers should call LogoutAsync to wipe storage.
    public event Action? ForceLogout;

    public void Set(AuthResponse resp)
    {
        Token = resp.Token;
        ExpiresAt = resp.ExpiresAt;
        User = new AuthUser(resp.UserId, resp.Username, resp.DisplayName, resp.Role, resp.AvatarUrl);
        Changed?.Invoke();
    }

    public void Hydrate(string token, DateTime expiresAt, AuthUser user)
    {
        Token = token;
        ExpiresAt = expiresAt;
        User = user;
        Changed?.Invoke();
    }

    public void Clear()
    {
        Token = null;
        ExpiresAt = null;
        User = null;
        Changed?.Invoke();
    }

    public void NotifyUnauthorized() => ForceLogout?.Invoke();
}

public record AuthUser(int Id, string Username, string DisplayName, UserRole Role, string? AvatarUrl);
