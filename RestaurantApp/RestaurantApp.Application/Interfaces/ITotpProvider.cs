namespace RestaurantApp.Application.Interfaces;

public interface ITotpProvider
{
    string GenerateSecretKey();
    string GenerateQrCodeUri(string email, string secretKey, string issuer = "RestaurantApp");
    byte[] GenerateQrCodeImage(string qrCodeUri);
    bool ValidateCode(string secretKey, string code);
}