using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.DTOs.Auth;
using RestaurantApp.Shared.DTOs.Users;

namespace RestaurantApp.Application.Interfaces.Services;

public interface IAuthService
{
    Task<Result> RegisterAsync(RegisterRequest request);
    Task<Result<LoginResponse>> LoginAsync(LoginRequest request, string ipAddress);
    Task<Result> LogoutAsync(string? refreshToken, string? accessToken, string ipAddress);
    Task<Result<ResponseUserDto>> GetCurrentUserAsync(string userId);
    Task<Result<List<ResponseUserDto>>> GetAllUsersAsync();
    Task<Result> ConfirmEmailAsync(string userId, string token);
    Task<Result> ResendEmailConfirmationAsync(string email);
}