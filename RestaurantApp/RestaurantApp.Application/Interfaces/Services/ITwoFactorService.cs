using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.DTOs.Auth.TwoFactor;

namespace RestaurantApp.Application.Interfaces.Services;

public interface ITwoFactorService
{
    Task<Result<Enable2FAResponse>> EnableTwoFactorAsync(CancellationToken ct = default);
    Task<Result> VerifyAndEnableAsync(string code, CancellationToken ct = default);
    Task<Result> DisableTwoFactorAsync(string code, CancellationToken ct = default);
}