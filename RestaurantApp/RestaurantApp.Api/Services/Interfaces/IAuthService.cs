using RestaurantApp.Api.Controllers;
using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.DTOs;
using RestaurantApp.Shared.DTOs.Auth;
using RestaurantApp.Shared.DTOs.Users;

namespace RestaurantApp.Api.Services.Interfaces;

public interface IAuthService
{
    Task<Result> RegisterAsync(RegisterRequest request);
    Task<Result<LoginResponse>> LoginAsync(LoginRequest request);
    Task<Result> LogoutAsync(string? presentedRefreshToken);
    Task<Result<ResponseUserDto>> GetCurrentUserAsync(string userId);
    Task<Result<List<ResponseUserDto>>> GetAllUsersAsync();
    //Task<Result> ChangePasswordAsync(string? userId, ChangePasswordRequest request);
    Task<Result> ConfirmEmailAsync(string userId, string token);

    Task<Result> ResendEmailConfirmationAsync(string email);
    //Task<Result> ForgotPasswordAsync(string email);
    //Task<Result> ResetPasswordAsync(ResetPasswordRequest request);
}


public class ClaimDto
{
    public string Type { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}