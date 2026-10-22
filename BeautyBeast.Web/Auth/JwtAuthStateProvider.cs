using System.Security.Claims;
using BeautyBeast.Web.Models;
using Microsoft.AspNetCore.Components.Authorization;

namespace BeautyBeast.Web.Auth;

public class JwtAuthStateProvider : AuthenticationStateProvider, IDisposable
{
    private readonly TokenStore _store;
    private static readonly AuthenticationState Anonymous =
        new(new ClaimsPrincipal(new ClaimsIdentity()));

    public JwtAuthStateProvider(TokenStore store)
    {
        _store = store;
        _store.Changed += OnChanged;
    }

    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        if (!_store.IsAuthenticated || _store.User is null)
            return Task.FromResult(Anonymous);

        var u = _store.User;
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, u.Id.ToString()),
            new(ClaimTypes.Name, u.Username),
            new(ClaimTypes.Role, u.Role.ToString()),
            new("displayName", u.DisplayName),
        };
        if (!string.IsNullOrEmpty(u.AvatarUrl))
            claims.Add(new Claim("avatarUrl", u.AvatarUrl));

        var identity = new ClaimsIdentity(claims, authenticationType: "jwt");
        return Task.FromResult(new AuthenticationState(new ClaimsPrincipal(identity)));
    }

    private void OnChanged() => NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());

    public void Dispose() => _store.Changed -= OnChanged;
}
