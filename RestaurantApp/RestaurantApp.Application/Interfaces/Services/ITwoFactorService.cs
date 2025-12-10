using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.DTOs.Auth.TwoFactor;

namespace RestaurantApp.Application.Interfaces.Services;

public interface ITwoFactorService
{
    Task<Result<Enable2FAResponse>> EnableTwoFactorAsync();
    Task<Result> VerifyAndEnableAsync(string code);
    Task<Result> DisableTwoFactorAsync(string code);
}