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
    Task<Result> LogoutAsync();
    Task<Result<ResponseUserDto>> GetCurrentUserAsync(string userId);
    Task<Result<List<ResponseUserDto>>> GetAllUsersAsync();
    Result GetDebugAuthInfo(bool isAuthenticated, string authenticationType, IEnumerable<ClaimDto> claims);
    Task<Result> ChangePasswordAsync(string? userId, ChangePasswordRequest request);
    Task<Result> ConfirmEmailAsync(string userId, string token);
    Task<Result> DeleteUserAsync(string userId);
}


public class ClaimDto
{
    public string Type { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}