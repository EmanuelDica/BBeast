using BeautyBeast.Web.Auth;
using BeautyBeast.Web.Models;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace BeautyBeast.Web.Services;

// Persists the JWT to the browser's session storage so it survives page reloads
// (but not closing the tab — appropriate for this app).
public class AuthService
{
    private const string TokenKey = "bb.token";
    private const string ExpiryKey = "bb.exp";
    private const string UserKey = "bb.user";

    private readonly ApiClient _api;
    private readonly TokenStore _tokens;
    private readonly ProtectedSessionStorage _storage;

    public AuthService(ApiClient api, TokenStore tokens, ProtectedSessionStorage storage)
    {
        _api = api;
        _tokens = tokens;
        _storage = storage;
    }

    public async Task<bool> TryRestoreSessionAsync()
    {
        try
        {
            var token = await _storage.GetAsync<string>(TokenKey);
            var expiry = await _storage.GetAsync<DateTime?>(ExpiryKey);
            var user = await _storage.GetAsync<AuthUser>(UserKey);
            if (token.Success && !string.IsNullOrWhiteSpace(token.Value)
                && expiry.Success && expiry.Value.HasValue
                && expiry.Value.Value > DateTime.UtcNow.AddSeconds(5)
                && user.Success && user.Value is not null)
            {
                _tokens.Hydrate(token.Value!, expiry.Value.Value, user.Value);
                return true;
            }
        }
        catch
        {
            // Pre-render or storage unavailable; ignore.
        }
        return false;
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest req)
    {
        var resp = await _api.PostAsync<AuthResponse>("/api/auth/login", req)
            ?? throw new InvalidOperationException("Empty login response");
        await PersistAsync(resp);
        return resp;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest req)
    {
        var resp = await _api.PostAsync<AuthResponse>("/api/auth/register", req)
            ?? throw new InvalidOperationException("Empty register response");
        await PersistAsync(resp);
        return resp;
    }

    public async Task LogoutAsync()
    {
        try { await _storage.DeleteAsync(TokenKey); } catch { }
        try { await _storage.DeleteAsync(ExpiryKey); } catch { }
        try { await _storage.DeleteAsync(UserKey); } catch { }
        _tokens.Clear();
    }

    public async Task<CurrentUser?> GetMeAsync()
    {
        if (!_tokens.IsAuthenticated) return null;
        try { return await _api.GetAsync<CurrentUser>("/api/auth/me"); }
        catch (ApiException) { return null; }
    }

    private async Task PersistAsync(AuthResponse resp)
    {
        _tokens.Set(resp);
        await _storage.SetAsync(TokenKey, resp.Token);
        await _storage.SetAsync(ExpiryKey, resp.ExpiresAt);
        await _storage.SetAsync(UserKey, new AuthUser(resp.UserId, resp.Username, resp.DisplayName, resp.Role, resp.AvatarUrl));
    }
}
