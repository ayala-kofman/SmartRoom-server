using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SmartRoom.Core.Entities;
using SmartRoom.Core.Interfaces.Services;
using SmartRoom.Core.Models;
using SmartRoom.Core.Options;

namespace SmartRoom.Service.Security;

/// <summary>
/// Creates the signed JWT returned at login (HMAC-SHA256).
/// The ID number is never put in the token: a token is readable by anyone who holds it.
/// </summary>
public class JwtTokenGenerator : IJwtTokenGenerator
{
    private readonly JwtOptions _options;
    private readonly TimeProvider _time;
    private readonly SigningCredentials _credentials;
    private readonly JwtSecurityTokenHandler _handler = new();

    public JwtTokenGenerator(IOptions<JwtOptions> options, TimeProvider time)
    {
        _options = options.Value;
        _time = time;

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Key));
        _credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
    }

    public (string Token, DateTime ExpiresAt) Generate(User user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString(CultureInfo.InvariantCulture)),
            new(ClaimTypes.Name, user.FullName),
            // ClaimTypes.Role, so [Authorize(Roles = "...")] works without extra mapping.
            new(ClaimTypes.Role, user.Role.ToString())
        };

        if (user.ClassGroupId is int classGroupId)
            claims.Add(new Claim(AppClaimTypes.ClassGroupId, classGroupId.ToString(CultureInfo.InvariantCulture)));

        var now = _time.GetUtcNow().UtcDateTime;
        var expiresAt = now.AddMinutes(_options.ExpiryMinutes);

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            notBefore: now,
            expires: expiresAt,
            signingCredentials: _credentials);

        return (_handler.WriteToken(token), expiresAt);
    }
}
