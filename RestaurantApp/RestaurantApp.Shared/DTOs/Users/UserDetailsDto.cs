namespace RestaurantApp.Shared.DTOs.Users;

public class UserDetailsDto
{
    public string Id { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    
    public string? PhoneNumber { get; set; }
    
    public bool TwoFactorEnabled { get; set; } 
    
    public bool EmailVerified { get; set; }
    
    public bool CanBeSearched { get; set; }
}