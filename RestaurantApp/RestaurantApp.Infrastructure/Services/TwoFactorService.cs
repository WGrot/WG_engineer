using OtpNet;
using QRCoder;
using RestaurantApp.Application.Interfaces.Repositories;
using RestaurantApp.Application.Interfaces.Services;
using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.DTOs.Auth.TwoFactor;

namespace RestaurantApp.Infrastructure.Services;

public class TwoFactorService: ITwoFactorService
{
    private readonly IEncryptionService _encryptionService;
    private readonly IUserRepository _userRepository;

    public TwoFactorService(IEncryptionService encryptionService, IUserRepository userRepository)
    {
        _encryptionService = encryptionService;
        _userRepository = userRepository;
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

    public async Task<Result<Enable2FAResponse>> EnableTwoFactorAsync(string userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            return Result<Enable2FAResponse>.Failure("User not found");

        var secretKey = GenerateSecretKey();
        var qrCodeUri = GenerateQrCodeUri(user.Email!, secretKey);
        var qrCodeImage = GenerateQrCodeImage(qrCodeUri);
        var encryptedSecretKey = _encryptionService.Encrypt(secretKey);

        user.TwoFactorSecretKey = encryptedSecretKey;
        await _userRepository.UpdateAsync(user);

        return Result<Enable2FAResponse>.Success(new Enable2FAResponse
        {
            SecretKey = secretKey,
            QrCodeUri = qrCodeUri,
            QrCodeImage = qrCodeImage
        });
    }

    public async Task<Result> VerifyAndEnableAsync(string userId, string code)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            return Result.Failure("User not found");

        if (string.IsNullOrEmpty(user.TwoFactorSecretKey))
            return Result.Failure("2FA has not been initialized");

        if (!ValidateCode(user.TwoFactorSecretKey, code))
            return Result.Failure("Invalid code");

        user.TwoFactorEnabled = true;
        await _userRepository.UpdateAsync(user);

        return Result.Success();
    }

    public async Task<Result> DisableTwoFactorAsync(string userId, string code)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            return Result.Failure("User not found");

        if (!user.TwoFactorEnabled)
            return Result.Failure("2FA is not enabled");

        if (!ValidateCode(user.TwoFactorSecretKey!, code))
            return Result.Failure("Invalid code");

        user.TwoFactorEnabled = false;
        user.TwoFactorSecretKey = null;
        await _userRepository.UpdateAsync(user);

        return Result.Success();
    }
}