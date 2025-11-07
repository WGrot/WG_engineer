using RestaurantApp.Shared.DTOs.Users;

namespace RestaurantApp.Shared.DTOs.Auth;

public class LoginResponse
{
    public string Token { get; set; } = string.Empty;
    public ResponseUserDto ResponseUser { get; set; } = new();
}