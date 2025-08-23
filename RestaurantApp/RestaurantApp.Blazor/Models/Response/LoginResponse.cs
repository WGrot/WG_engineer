using RestaurantApp.Blazor.Models.DTO;

namespace RestaurantApp.Blazor.Models.Response;

public class LoginResponse
{
    public string Token { get; set; } = string.Empty;
    public UserDto User { get; set; } = new();
}