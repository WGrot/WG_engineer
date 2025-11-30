using OtpNet;
using QRCoder;
using RestaurantApp.Api.Services.Interfaces;
using RestaurantApp.Application.Interfaces.Services;

namespace RestaurantApp.Api.Services;

public class TwoFactorService: ITwoFactorService
{
    private readonly IEncryptionService _encryptionService;
    
    public TwoFactorService(IEncryptionService aesEncryptionService)
    {
        _encryptionService = aesEncryptionService;
    }
    public string GenerateSecretKey()
    {
        var key = KeyGeneration.GenerateRandomKey(20);
        return Base32Encoding.ToString(key);
    }

    public string GenerateQrCodeUri(string email, string secretKey, string issuer = "RestaurantApp")
    {
        return $"otpauth://totp/{issuer}:{email}?secret={secretKey}&issuer={issuer}";
    }

    public byte[] GenerateQrCodeImage(string qrCodeUri)
    {
        using var qrGenerator = new QRCodeGenerator();
        using var qrCodeData = qrGenerator.CreateQrCode(qrCodeUri, QRCodeGenerator.ECCLevel.Q);
        using var qrCode = new PngByteQRCode(qrCodeData);
        return qrCode.GetGraphic(20);
    }

    public bool ValidateCode(string encryptedKey, string code)
    {
        
        var decryptedSecretKey = Base32Encoding.ToBytes(_encryptionService.Decrypt(encryptedKey));
        
        var totp = new Totp(decryptedSecretKey);
        return totp.VerifyTotp(code, out _, new VerificationWindow(2, 2));
    }
}