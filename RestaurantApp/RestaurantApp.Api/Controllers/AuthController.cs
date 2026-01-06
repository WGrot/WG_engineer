using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RestaurantApp.Api.Common;
using RestaurantApp.Application.Interfaces.Services;
using RestaurantApp.Domain.Models;
using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.DTOs.Auth;
using RestaurantApp.Shared.DTOs.Auth.TwoFactor;

namespace RestaurantApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ITokenService _tokenService;
    private readonly IConfiguration _configuration;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public AuthController(
        IAuthService authService,
        ITokenService tokenService,
        IConfiguration configuration,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        _authService = authService;
        _tokenService = tokenService;
        _configuration = configuration;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var result = await _authService.RegisterAsync(request);
        return result.ToActionResult();
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await _authService.LoginAsync(request, GetIpAddress());

        if (result.IsFailure)
            return StatusCode(result.StatusCode, result.Error);

        var data = result.Value!;

        var user = await _userManager.FindByEmailAsync(request.Email);
        var (refreshToken, refreshExpiresAt) = await _tokenService.GenerateRefreshTokenAsync(user!, GetIpAddress());
        if (!data.RequiresTwoFactor)
            SetRefreshTokenCookie(refreshToken, refreshExpiresAt);

        return result.ToActionResult();
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh()
    {
        if (!TryGetRefreshToken(out var refreshToken))
            return Unauthorized("Refresh token not found");

        var (success, newAccess, newRefresh) =
            await _tokenService.ValidateAndRotateRefreshTokenAsync(refreshToken!, GetIpAddress());

        if (!success)
        {
            DeleteRefreshTokenCookie();
            return Unauthorized("Invalid refresh token");
        }

        var refreshDays = _configuration.GetValue("JwtConfig:RefreshTokenDays", 14);
        SetRefreshTokenCookie(newRefresh!, DateTime.UtcNow.AddDays(refreshDays));

        return Result.Success(new RefreshResponse { Token = newAccess! }).ToActionResult();
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        TryGetRefreshToken(out var refreshToken);

        var accessToken = Request.Headers["Authorization"]
            .FirstOrDefault()?.Replace("Bearer ", "");

        await _authService.LogoutAsync(refreshToken, accessToken, GetIpAddress());

        DeleteRefreshTokenCookie();

        return Ok();
    }

    [HttpGet("confirm-email")]
    [AllowAnonymous]
    public async Task<IActionResult> ConfirmEmail([FromQuery] string userId, [FromQuery] string token)
    {
        var result = await _authService.ConfirmEmailAsync(userId, token);
        var frontendUrl = _configuration["AppURL:FrontendUrl"];

        return result.IsSuccess
            ? Redirect($"{frontendUrl}/email-verified")
            : BadRequest(result);
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetCurrentUser()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var result = await _authService.GetCurrentUserAsync(userId!);
        return result.ToActionResult();
    }

    [HttpGet("users")]
    public async Task<IActionResult> GetAllUsers()
    {
        var result = await _authService.GetAllUsersAsync();
        return result.ToActionResult();
    }

    [HttpPost("resend-confirmation-email")]
    [AllowAnonymous]
    public async Task<IActionResult> ResendConfirmationEmail([FromBody] ResendEmailRequest request)
    {
        var result = await _authService.ResendEmailConfirmationAsync(request.Email);
        return result.ToActionResult();
    }
    
    [HttpGet("test-exception")]
    public IActionResult TestException()
    {
        throw new Exception("Test exception - middleware works!");
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("make-admin/{userId}")]
    public async Task<IActionResult> MakeAdmin(string userId)
    {
        if (!await _roleManager.RoleExistsAsync("Admin"))
        {
            await _roleManager.CreateAsync(new IdentityRole("Admin"));
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user != null)
        {
            await _userManager.AddToRoleAsync(user, "Admin");
        }
        return Ok();
    }

    #region Cookie Helpers

    private string GetIpAddress()
        => HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

    private string CookieName
        => _configuration["JwtConfig:RefreshCookieName"] ?? "refreshToken";

    private bool TryGetRefreshToken(out string? token)
        => Request.Cookies.TryGetValue(CookieName, out token) && !string.IsNullOrEmpty(token);

    private void SetRefreshTokenCookie(string token, DateTime expires)
    {
        Response.Cookies.Append(CookieName, token, new CookieOptions
        {
            HttpOnly = true,
            Secure = !_configuration.GetValue<bool>("Kestrel:AllowInsecureCookies"),
            SameSite = SameSiteMode.Lax,
            Expires = expires,
            Path = "/"
        });
    }

    private void DeleteRefreshTokenCookie()
    {
        Response.Cookies.Delete(CookieName);
        Response.Cookies.Append(CookieName, "", new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            MaxAge = TimeSpan.Zero,
            Path = "/"
        });
    }

    #endregion
}