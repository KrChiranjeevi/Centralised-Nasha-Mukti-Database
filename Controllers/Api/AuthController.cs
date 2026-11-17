using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NashaMuktiDB.Data;
using NashaMuktiDB.DTOs;
using NashaMuktiDB.Models;
using NashaMuktiDB.Services;

namespace NashaMuktiDB.Controllers.Api;

[Route("api/v1/auth")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ITokenService _tokenService;
    private readonly ApplicationDbContext _context;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        ITokenService tokenService,
        ApplicationDbContext context,
        ILogger<AuthController> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _tokenService = tokenService;
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Authenticate credentials and receive a JWT Bearer token for API access.
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<string>.Fail("Invalid credentials format"));
        }

        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null || !user.IsActive)
        {
            _logger.LogWarning("Failed login attempt for non-existent or inactive user: {Email}", request.Email);
            return Unauthorized(ApiResponse<string>.Fail("Invalid email or password"));
        }

        var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);
        if (!result.Succeeded)
        {
            if (result.IsLockedOut)
            {
                return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<string>.Fail("Account is temporarily locked out due to multiple failed attempts."));
            }
            return Unauthorized(ApiResponse<string>.Fail("Invalid email or password"));
        }

        var roles = await _userManager.GetRolesAsync(user);
        var primaryRole = roles.FirstOrDefault() ?? "CentreStaff";

        string? centreName = null;
        if (user.CentreId.HasValue)
        {
            centreName = await _context.Centres
                .Where(c => c.CentreId == user.CentreId.Value)
                .Select(c => c.CentreName)
                .FirstOrDefaultAsync();
        }

        var token = _tokenService.GenerateJwtToken(user, primaryRole, centreName);

        _logger.LogInformation("API user {Email} logged in successfully as {Role}", user.Email, primaryRole);

        return Ok(new AuthResponseDto
        {
            Success = true,
            Message = "Authentication successful",
            Token = token,
            TokenType = "Bearer",
            ExpiresInSeconds = _tokenService.ExpirySeconds,
            User = new UserProfileDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email ?? string.Empty,
                Role = primaryRole,
                CentreId = user.CentreId,
                CentreName = centreName
            }
        });
    }

    /// <summary>
    /// Retrieve current authenticated user profile and assigned permissions.
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<UserProfileDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetCurrentUser()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return Unauthorized();

        var roles = await _userManager.GetRolesAsync(user);
        var primaryRole = roles.FirstOrDefault() ?? "CentreStaff";

        string? centreName = null;
        if (user.CentreId.HasValue)
        {
            centreName = await _context.Centres
                .Where(c => c.CentreId == user.CentreId.Value)
                .Select(c => c.CentreName)
                .FirstOrDefaultAsync();
        }

        return Ok(ApiResponse<UserProfileDto>.Ok(new UserProfileDto
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email ?? string.Empty,
            Role = primaryRole,
            CentreId = user.CentreId,
            CentreName = centreName
        }));
    }
}
