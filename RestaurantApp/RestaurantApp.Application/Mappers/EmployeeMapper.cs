using RestaurantApp.Application.Mappers.EnumMappers;
using RestaurantApp.Domain.Models;
using RestaurantApp.Shared.DTOs.Employees;

namespace RestaurantApp.Application.Mappers;

public static class EmployeeMapper
{
    public static RestaurantEmployeeDto ToDto(this RestaurantEmployee entity)
    {
        return new RestaurantEmployeeDto
        {
            Id = entity.Id,
            UserId = entity.UserId,
            RestaurantId = entity.RestaurantId,
            Restaurant = entity.Restaurant.ToDto(),
            RoleEnumDto = entity.Role.ToShared(),
            Permissions = entity.Permissions.Select(p => p.ToDto()).ToList() ,
            CreatedAt = entity.CreatedAt,
            IsActive = entity.IsActive
        };
    }


    public static RestaurantEmployee ToEntity(this RestaurantEmployeeDto dto)
    {
        return new RestaurantEmployee
        {
            Id = dto.Id,
            UserId = dto.UserId,
            RestaurantId = dto.RestaurantId,
            Role = dto.RoleEnumDto.ToDomain(),
            Permissions = dto.Permissions.Select(p => p.ToEntity()).ToList(),
            CreatedAt = dto.CreatedAt,
            IsActive = dto.IsActive
        };
    }
    
    
    public static List<RestaurantEmployeeDto> ToDtoList(this IEnumerable<RestaurantEmployee> entities)
    {
        return entities.Select(e => e.ToDto()).ToList();
    }
    
    public static List<RestaurantEmployee> ToEntityList(this IEnumerable<RestaurantEmployeeDto> dtos)
    {
        return dtos.Select(d => d.ToEntity()).ToList();
    }
    
    public static void SetUserData(this RestaurantEmployeeDto dto, string email, string firstName, string lastName, string phoneNumber)
    {
        dto.Email = email;
        dto.FirstName = firstName;
        dto.LastName = lastName;
        dto.PhoneNumber = phoneNumber;
    }
}