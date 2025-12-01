using Microsoft.AspNetCore.Identity;

namespace RestaurantApp.Domain.Models;



public class ApplicationUser : IdentityUser
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;


    public bool TwoFactorEnabled { get; set; } = false;
    public string? TwoFactorSecretKey { get; set; }

    public DateTime? LastLoginAt { get; set; }
    public bool IsActive { get; set; } = true;
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiryTime { get; set; }
    

    public List<ReservationBase>? Reservations { get; set; } = new();
    

    public string? RestaurantId { get; set; } 
    
    public void UpdateLastLogin()
    {
        LastLoginAt = DateTime.UtcNow;
    }
}