using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.DTOs.Auth;

namespace RestaurantApp.Application.Interfaces.Services;

public interface IPasswordService
{
    string GenerateSecurePassword(int length = 16);
    Task<Result> ForgotPasswordAsync(string email);
    Task<Result> ResetPasswordAsync(ResetPasswordRequest request);
    Task<Result> ChangePasswordAsync(ChangePasswordRequest request);
}
