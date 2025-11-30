namespace RestaurantApp.Application.Interfaces.Services;

public interface ITwoFactorService
{
    string GenerateSecretKey();
    string GenerateQrCodeUri(string email, string secretKey, string issuer = "RestaurantApp");
    byte[] GenerateQrCodeImage(string qrCodeUri);
    bool ValidateCode(string encryptedKey, string code);
}