using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.DTOs.Auth.TwoFactor;

namespace RestaurantApp.Application.Interfaces.Services;

public interface ITwoFactorService
{
    string GenerateSecretKey();
    string GenerateQrCodeUri(string email, string secretKey, string issuer = "RestaurantApp");
    byte[] GenerateQrCodeImage(string qrCodeUri);
    bool ValidateCode(string encryptedKey, string code);
    
    Task<Result<Enable2FAResponse>> EnableTwoFactorAsync();
    Task<Result> VerifyAndEnableAsync(string code);
    Task<Result> DisableTwoFactorAsync(string code);
}