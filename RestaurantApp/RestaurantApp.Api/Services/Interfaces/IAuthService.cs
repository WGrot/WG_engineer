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
    Task<Result> ConfirmEmailAsync(string userId, string token);

    Task<Result> ResendEmailConfirmationAsync(string email);
}
