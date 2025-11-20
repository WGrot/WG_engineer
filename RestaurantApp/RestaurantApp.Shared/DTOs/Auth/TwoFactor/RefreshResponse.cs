using RestaurantApp.Shared.DTOs.Users;

namespace RestaurantApp.Shared.DTOs.Auth.TwoFactor;

public class RefreshResponse
{
    public string Token { get; set; } = string.Empty;
    public ResponseUserLoginDto? User { get; set; }
}