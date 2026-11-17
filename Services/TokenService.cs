using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using NashaMuktiDB.Models;

namespace NashaMuktiDB.Services;

public class TokenService : ITokenService
{
    private readonly IConfiguration _config;
    private readonly string _key;
    private readonly string _issuer;
    private readonly string _audience;
    private readonly int _expiryHours;

    public int ExpirySeconds => _expiryHours * 3600;

    public TokenService(IConfiguration config)
    {
        _config = config;
        _key = _config["Jwt:Key"] ?? "NashaMuktiPlatform_SecureJwtSecretKey_2026_AcademicDemoKey99!";
        _issuer = _config["Jwt:Issuer"] ?? "NashaMuktiPortal";
        _audience = _config["Jwt:Audience"] ?? "NashaMuktiClients";
        _expiryHours = int.TryParse(_config["Jwt:ExpiryHours"], out var hours) ? hours : 8;
    }

    public string GenerateJwtToken(ApplicationUser user, string role, string? centreName)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_key));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id),
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Name, user.UserName ?? user.Email ?? string.Empty),
            new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
            new Claim(ClaimTypes.Role, role),
            new Claim("FullName", user.FullName),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        if (user.CentreId.HasValue)
        {
            claims.Add(new Claim("CentreId", user.CentreId.Value.ToString()));
            if (!string.IsNullOrEmpty(centreName))
            {
                claims.Add(new Claim("CentreName", centreName));
            }
        }

        var expires = DateTime.UtcNow.AddHours(_expiryHours);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = expires,
            Issuer = _issuer,
            Audience = _audience,
            SigningCredentials = credentials
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);
    }
}
