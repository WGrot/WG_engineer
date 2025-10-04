using RestaurantApp.Api.Controllers;
using RestaurantApp.Api.Models.DTOs;
using RestaurantApp.Shared.Common;

namespace RestaurantApp.Api.Services.Interfaces;

public interface IAuthService
{
    Task<Result> RegisterAsync(RegisterRequest request);
    Task<Result<LoginResponse>> LoginAsync(LoginRequest request);
    Task<Result> LogoutAsync();
    Task<Result<ResponseUserDto>> GetCurrentUserAsync(string userId);
    Task<Result<List<ResponseUserDto>>> GetAllUsersAsync();
    Result GetDebugAuthInfo(bool isAuthenticated, string authenticationType, IEnumerable<ClaimDto> claims);
}

public class RegisterRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
}

public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class LoginResponse
{
    public string Token { get; set; } = string.Empty;
    public ResponseUserDto ResponseUser { get; set; } = new();
}

public class GetAllUsersResponse
{
    public string Message { get; set; } = string.Empty;
    public int Count { get; set; }
    public List<ResponseUserDto> Users { get; set; } = new();
}

public class ClaimDto
{
    public string Type { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}