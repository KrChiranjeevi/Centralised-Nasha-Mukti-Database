using NashaMuktiDB.Models;

namespace NashaMuktiDB.Services;

public interface ITokenService
{
    string GenerateJwtToken(ApplicationUser user, string role, string? centreName);
    int ExpirySeconds { get; }
}

