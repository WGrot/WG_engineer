using Microsoft.AspNetCore.Identity;

namespace RestaurantApp.Shared.Models;



public class ApplicationUser : IdentityUser
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Dodatkowe przydatne właściwości:
    public DateTime? LastLoginAt { get; set; }
    public bool IsActive { get; set; } = true;
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiryTime { get; set; }
    
    
    // Jeśli aplikacja ma role restauracyjne
    public string? RestaurantId { get; set; } // Dla powiązania z konkretną restauracją
    
    // Metoda do aktualizacji czasu ostatniego logowania
    public void UpdateLastLogin()
    {
        LastLoginAt = DateTime.UtcNow;
    }
}