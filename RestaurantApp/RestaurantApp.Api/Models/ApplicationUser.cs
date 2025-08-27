using Microsoft.AspNetCore.Identity;
using RestaurantApp.Shared.Models;

namespace RestaurantApp.Api;



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
    
    //Relacja do rezerwacji
    public List<ReservationBase>? Reservations { get; set; } = new();
    
    // Jeśli aplikacja ma role restauracyjne
    public string? RestaurantId { get; set; } // Dla powiązania z konkretną restauracją
    
    // Metoda do aktualizacji czasu ostatniego logowania
    public void UpdateLastLogin()
    {
        LastLoginAt = DateTime.UtcNow;
    }
}