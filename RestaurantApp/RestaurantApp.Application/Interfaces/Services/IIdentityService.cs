using RestaurantApp.Domain.Models;
using RestaurantApp.Shared.Common;

namespace RestaurantApp.Application.Interfaces.Services;

public interface IIdentityService
{
    Task<Result<ApplicationUser>> CreateUserAsync(ApplicationUser user, string password);
    
    // Password operations
    Task<bool> CheckPasswordAsync(ApplicationUser user, string password);
    Task<Result> ChangePasswordAsync(ApplicationUser user, string currentPassword, string newPassword);
    
    // Email confirmation
    Task<string> GenerateEmailConfirmationTokenAsync(ApplicationUser user);
    Task<Result> ConfirmEmailAsync(ApplicationUser user, string token);
    
    // Password reset
    Task<string> GeneratePasswordResetTokenAsync(ApplicationUser user);
    Task<Result> ResetPasswordAsync(ApplicationUser user, string token, string newPassword);
    
    // Lockout
    Task<bool> IsLockedOutAsync(ApplicationUser user);
    Task RecordFailedAccessAsync(ApplicationUser user);
    Task ResetAccessFailedCountAsync(ApplicationUser user);
    
    // Roles (if needed)
    Task<IList<string>> GetRolesAsync(ApplicationUser user);
    Task<Result> AddToRoleAsync(ApplicationUser user, string role);
}