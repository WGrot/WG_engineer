using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.DTOs.Auth;

namespace RestaurantApp.Application.Interfaces.Services;

public interface IPasswordService
{
    string GenerateSecurePassword(int length = 16);
    Task<Result> ForgotPasswordAsync(string email, CancellationToken ct = default);
    Task<Result> ResetPasswordAsync(ResetPasswordRequest request, CancellationToken ct = default);
    Task<Result> ChangePasswordAsync(string userId, ChangePasswordRequest request, CancellationToken ct = default);
}
