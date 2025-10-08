namespace RestaurantApp.Shared.DTOs;

public class LoginResponse
{
    public string Token { get; set; } = string.Empty;
    public ResponseUserDto ResponseUser { get; set; } = new();
}