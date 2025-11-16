using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantApp.Api.Services.Interfaces;
using RestaurantApp.Shared.DTOs.Auth.TwoFactor;

namespace RestaurantApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TwoFactorController : ControllerBase
{
    private readonly ApiDbContext _context;
    private readonly ITwoFactorService _twoFactorService;
    private readonly IAesEncryptionService _encryptionService;

    public TwoFactorController(ApiDbContext context, ITwoFactorService twoFactorService, IAesEncryptionService encryptionService)
    {
        _context = context;
        _twoFactorService = twoFactorService;
        _encryptionService = encryptionService;
    }

    [HttpPost("enable")]
    [Authorize]
    public async Task<ActionResult<Enable2FAResponse>> EnableTwoFactor()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var user = await _context.Users.FindAsync(userId);
        if (user == null)
            return NotFound();

        // Generuj nowy klucz
        var secretKey = _twoFactorService.GenerateSecretKey();
        var qrCodeUri = _twoFactorService.GenerateQrCodeUri(user.Email!, secretKey);
        var qrCodeImage = _twoFactorService.GenerateQrCodeImage(qrCodeUri);
        var encryptedSecretKey = _encryptionService.Encrypt(secretKey);

        // Zapisz klucz (2FA nie jest jeszcze aktywne, dopóki nie zostanie zweryfikowane)
        user.TwoFactorSecretKey = encryptedSecretKey;
        await _context.SaveChangesAsync();

        return Ok(new Enable2FAResponse
        {
            SecretKey = secretKey,
            QrCodeUri = qrCodeUri,
            QrCodeImage = qrCodeImage
        });
    }

    [HttpPost("verify-and-enable")]
    [Authorize]
    public async Task<ActionResult> VerifyAndEnable([FromBody] Verify2FARequest request)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var user = await _context.Users.FindAsync(userId);
        if (user == null || string.IsNullOrEmpty(user.TwoFactorSecretKey))
            return BadRequest("2FA nie zostało zainicjalizowane");

        // Weryfikuj kod
        if (!_twoFactorService.ValidateCode(user.TwoFactorSecretKey, request.Code))
            return BadRequest("Nieprawidłowy kod");

        // Aktywuj 2FA
        user.TwoFactorEnabled = true;
        await _context.SaveChangesAsync();

        return Ok(new { message = "2FA zostało pomyślnie włączone" });
    }

    [HttpPost("disable")]
    [Authorize]
    public async Task<ActionResult> DisableTwoFactor([FromBody] Verify2FARequest request)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var user = await _context.Users.FindAsync(userId);
        if (user == null || !user.TwoFactorEnabled)
            return BadRequest("2FA nie jest włączone");

        // Weryfikuj kod przed wyłączeniem
        if (!_twoFactorService.ValidateCode(user.TwoFactorSecretKey!, request.Code))
            return BadRequest("Nieprawidłowy kod");

        user.TwoFactorEnabled = false;
        user.TwoFactorSecretKey = null;
        await _context.SaveChangesAsync();

        return Ok(new { message = "2FA zostało wyłączone" });
    }
}