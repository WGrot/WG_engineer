using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.DTOs.Auth;
using RestaurantApp.Shared.DTOs.Users;

namespace RestaurantApp.Application.Interfaces.Services;

public interface IAuthService
{
    Task<Result> RegisterAsync(RegisterRequest request, CancellationToken ct = default);
    Task<Result<LoginResponse>> LoginAsync(LoginRequest request, string ipAddress, CancellationToken ct = default);
    Task<Result> LogoutAsync(string? refreshToken, string? accessToken, string ipAddress, CancellationToken ct = default);
    Task<Result<ResponseUserDto>> GetCurrentUserAsync(string userId, CancellationToken ct = default);
    Task<Result<List<ResponseUserDto>>> GetAllUsersAsync(CancellationToken ct = default);
    Task<Result> ConfirmEmailAsync(string userId, string token, CancellationToken ct = default);
    Task<Result> ResendEmailConfirmationAsync(string email, CancellationToken ct = default);
}