using RestaurantApp.Application.Interfaces;
using RestaurantApp.Application.Interfaces.Repositories;
using RestaurantApp.Application.Interfaces.Services;
using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.DTOs.Auth.TwoFactor;

namespace RestaurantApp.Application.Services;

public class TwoFactorService: ITwoFactorService
{
    private readonly IEncryptionService _encryptionService;
    private readonly IUserRepository _userRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ITotpProvider _totpProvider;

    public TwoFactorService(IEncryptionService encryptionService, IUserRepository userRepository, ICurrentUserService currentUserService, ITotpProvider totpProvider)
    {
        _encryptionService = encryptionService;
        _userRepository = userRepository;
        _currentUserService = currentUserService;
        _totpProvider = totpProvider;
    }
    

    public async Task<Result<Enable2FAResponse>> EnableTwoFactorAsync(CancellationToken ct)
    {
        var userId = _currentUserService.UserId;
        var user = await _userRepository.GetByIdAsync(userId!, ct);
        if (user == null)
            return Result<Enable2FAResponse>.Failure("User not found");

        var secretKey = _totpProvider.GenerateSecretKey();
        var qrCodeUri = _totpProvider.GenerateQrCodeUri(user.Email!, secretKey);
        var qrCodeImage = _totpProvider.GenerateQrCodeImage(qrCodeUri);
        var encryptedSecretKey = _encryptionService.Encrypt(secretKey);

        user.TwoFactorSecretKey = encryptedSecretKey;
        await _userRepository.UpdateAsync(user, ct);

        return Result<Enable2FAResponse>.Success(new Enable2FAResponse
        {
            SecretKey = secretKey,
            QrCodeUri = qrCodeUri,
            QrCodeImage = qrCodeImage
        });
    }

    public async Task<Result> VerifyAndEnableAsync(string code, CancellationToken ct)
    {
        var userId = _currentUserService.UserId;
        var user = await _userRepository.GetByIdAsync(userId!, ct);
        if (user == null)
            return Result.Failure("User not found");

        if (string.IsNullOrEmpty(user.TwoFactorSecretKey))
            return Result.Failure("2FA has not been initialized");

        var decryptedKey = _encryptionService.Decrypt(user.TwoFactorSecretKey);
        if (!_totpProvider.ValidateCode(decryptedKey, code))
            return Result.Failure("Invalid code");

        user.TwoFactorEnabled = true;
        await _userRepository.UpdateAsync(user, ct);

        return Result.Success();
    }

    public async Task<Result> DisableTwoFactorAsync(string code, CancellationToken ct)
    {
        var userId = _currentUserService.UserId;
        var user = await _userRepository.GetByIdAsync(userId!, ct);
        if (user == null)
            return Result.Failure("User not found");

        if (!user.TwoFactorEnabled)
            return Result.Failure("2FA is not enabled");

        var decryptedKey = _encryptionService.Decrypt(user.TwoFactorSecretKey!);
        if (!_totpProvider.ValidateCode(decryptedKey, code))
            return Result.Failure("Invalid code");

        user.TwoFactorEnabled = false;
        user.TwoFactorSecretKey = null;
        await _userRepository.UpdateAsync(user, ct);

        return Result.Success();
    }
}