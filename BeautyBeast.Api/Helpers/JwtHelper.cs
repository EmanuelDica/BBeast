using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BeautyBeast.Api.Entities;
using Microsoft.IdentityModel.Tokens;

namespace BeautyBeast.Api.Helpers;

public class JwtHelper
{
    private readonly IConfiguration _config;

    public JwtHelper(IConfiguration config)
    {
        _config = config;
    }

    public (string token, DateTime expiresAt) Issue(User user)
    {
        var key = _config["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key missing");
        var issuer = _config["Jwt:Issuer"] ?? "BeautyBeast.Api";
        var audience = _config["Jwt:Audience"] ?? "BeautyBeast.Web";
        var hours = int.TryParse(_config["Jwt:ExpiresHours"], out var h) ? h : 12;

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
            new Claim("displayName", user.DisplayName),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var creds = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
            SecurityAlgorithms.HmacSha256);

        var expires = DateTime.UtcNow.AddHours(hours);
        var jwt = new JwtSecurityToken(issuer, audience, claims, expires: expires, signingCredentials: creds);
        return (new JwtSecurityTokenHandler().WriteToken(jwt), expires);
    }
}
