using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.DTOs.Auth;

namespace RestaurantApp.Api.Services.Interfaces;

public interface IPasswordService
{
    Task<Result> ForgotPasswordAsync(string email);
    Task<Result> ResetPasswordAsync(ResetPasswordRequest request);
    Task<Result> ChangePasswordAsync(string? userId, ChangePasswordRequest request);
    string GenerateSecurePassword(int length = 16);
}