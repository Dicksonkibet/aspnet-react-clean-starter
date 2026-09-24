using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CleanStart.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace CleanStart.API.Auth;

/// <summary>Small, single-purpose class: turns a validated user into a signed JWT.</summary>
public class JwtTokenGenerator
{
    private static readonly TimeSpan TokenLifetime = TimeSpan.FromHours(2);

    private readonly IConfiguration _config;
    private readonly UserManager<ApplicationUser> _userManager;

    public JwtTokenGenerator(IConfiguration config, UserManager<ApplicationUser> userManager)
    {
        _config = config;
        _userManager = userManager;
    }

    public async Task<(string Token, DateTime ExpiresAtUtc)> GenerateTokenAsync(ApplicationUser user)
    {
        var jwtSection = _config.GetSection("Jwt");
        var roles = await _userManager.GetRolesAsync(user);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new("fullName", user.FullName)
        };
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSection["Key"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.UtcNow.Add(TokenLifetime);

        var token = new JwtSecurityToken(
            issuer: jwtSection["Issuer"],
            audience: jwtSection["Audience"],
            claims: claims,
            expires: expires,
            signingCredentials: credentials);

        return (new JwtSecurityTokenHandler().WriteToken(token), expires);
    }
}
