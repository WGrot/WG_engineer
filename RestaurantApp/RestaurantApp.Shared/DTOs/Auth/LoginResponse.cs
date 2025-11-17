using RestaurantApp.Shared.DTOs.Users;

namespace RestaurantApp.Shared.DTOs.Auth;

public class LoginResponse
{
    public string Token { get; set; } = string.Empty;
    public ResponseUserLoginDto ResponseUser { get; set; } = new();
    
    public bool RequiresTwoFactor { get; set; }
    
    public bool IsEmailVeryfied { get; set; }
}