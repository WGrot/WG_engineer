using RestaurantApp.Domain.Models;
using RestaurantApp.Shared.DTOs.Permissions;

namespace RestaurantApp.Api.Mappers;

public static class PermissionMapper
{
    // Z Entity na DTO
    public static RestaurantPermissionDto ToDto(this RestaurantPermission entity)
    {
        return new RestaurantPermissionDto
        {
            Id = entity.Id,
            RestaurantEmployeeId = entity.RestaurantEmployeeId,
            Permission = entity.Permission
        };
    }

    // Z DTO na Entity
    public static RestaurantPermission ToEntity(this RestaurantPermissionDto dto)
    {
        return new RestaurantPermission
        {
            Id = dto.Id,
            RestaurantEmployeeId = dto.RestaurantEmployeeId,
            Permission = dto.Permission
            // RestaurantEmployee będzie uzupełniony przez EF
        };
    }

    // Aktualizacja istniejącej encji z DTO
    public static void UpdateFromDto(this RestaurantPermission entity, RestaurantPermissionDto dto)
    {
        entity.RestaurantEmployeeId = dto.RestaurantEmployeeId;
        entity.Permission = dto.Permission;
        // Nie aktualizujemy Id, bo to klucz główny
    }

    // Mapowanie kolekcji Entity na listę DTO
    public static List<RestaurantPermissionDto> ToDtoList(this IEnumerable<RestaurantPermission> entities)
    {
        return entities.Select(e => e.ToDto()).ToList();
    }

    // Mapowanie kolekcji DTO na listę Entity
    public static List<RestaurantPermission> ToEntityList(this IEnumerable<RestaurantPermissionDto> dtos)
    {
        return dtos.Select(d => d.ToEntity()).ToList();
    }
    
    public static RestaurantPermission ToEntity(this CreateRestaurantPermissionDto dto)
    {
        return new RestaurantPermission
        {
            RestaurantEmployeeId = dto.RestaurantEmployeeId,
            Permission = dto.Permission
        };
    }
}