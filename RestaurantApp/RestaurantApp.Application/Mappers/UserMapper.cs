using RestaurantApp.Domain.Models;
using RestaurantApp.Shared.DTOs.Users;

namespace RestaurantApp.Application.Mappers;

public static class UserMapper
{
    public static void UpdateFromDto(this ApplicationUser entity, UpdateUserDto dto)
    {
        entity.FirstName = dto.FirstName;
        entity.LastName = dto.LastName;
        entity.PhoneNumber = dto.PhoneNumber;

    }
    
    public static UserDetailsDto MapToDetaisDto(this ApplicationUser entity )
    {
        UserDetailsDto dto = new()
        {
            Id = entity.Id,
            Email = entity.Email,
            FirstName = entity.FirstName,
            LastName = entity.LastName,
            PhoneNumber = entity.PhoneNumber,
            TwoFactorEnabled = entity.TwoFactorEnabled,
            EmailVerified = entity.EmailConfirmed,
            CanBeSearched = entity.CanBeSearched
        };
        
        return dto;
    }
}