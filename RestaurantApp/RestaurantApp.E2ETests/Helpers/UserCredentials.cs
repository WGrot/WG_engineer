namespace RestaurantApp.E2ETests.Helpers;

public class UserCredentials
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? TotpSecret { get; set; }
}