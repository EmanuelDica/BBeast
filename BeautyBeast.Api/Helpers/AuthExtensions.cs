using System.Security.Claims;
using BeautyBeast.Api.Entities;

namespace BeautyBeast.Api.Helpers;

public static class AuthExtensions
{
    public static int? UserId(this ClaimsPrincipal user)
    {
        var raw = user.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(raw, out var id) ? id : null;
    }

    public static UserRole? UserRoleValue(this ClaimsPrincipal user)
    {
        var raw = user.FindFirstValue(ClaimTypes.Role);
        return Enum.TryParse<UserRole>(raw, out var r) ? r : null;
    }

    public static int RequireUserId(this ClaimsPrincipal user) =>
        user.UserId() ?? throw new UnauthorizedAccessException();
}
