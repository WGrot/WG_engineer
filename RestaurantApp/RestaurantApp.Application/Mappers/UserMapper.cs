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
}