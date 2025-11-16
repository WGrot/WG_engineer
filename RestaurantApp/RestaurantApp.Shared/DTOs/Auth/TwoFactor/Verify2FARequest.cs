namespace RestaurantApp.Shared.DTOs.Auth.TwoFactor;

public class Verify2FARequest
{
    public string UserId { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
}