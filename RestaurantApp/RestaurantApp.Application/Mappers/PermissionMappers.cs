using RestaurantApp.Application.Mappers.EnumMappers;
using RestaurantApp.Domain.Models;
using RestaurantApp.Shared.DTOs.Permissions;

namespace RestaurantApp.Application.Mappers;

public static class PermissionMapper
{

    public static RestaurantPermissionDto ToDto(this RestaurantPermission entity)
    {
        return new RestaurantPermissionDto
        {
            Id = entity.Id,
            RestaurantEmployeeId = entity.RestaurantEmployeeId,
            Permission = entity.Permission.ToShared()
        };
    }


    public static RestaurantPermission ToEntity(this RestaurantPermissionDto dto)
    {
        return new RestaurantPermission
        {
            Id = dto.Id,
            RestaurantEmployeeId = dto.RestaurantEmployeeId,
            Permission = dto.Permission.ToDomain()

        };
    }
    
    public static void UpdateFromDto(this RestaurantPermission entity, RestaurantPermissionDto dto)
    {
        entity.RestaurantEmployeeId = dto.RestaurantEmployeeId;
        entity.Permission = dto.Permission.ToDomain();

    }

    public static List<RestaurantPermissionDto> ToDtoList(this IEnumerable<RestaurantPermission> entities)
    {
        return entities.Select(e => e.ToDto()).ToList();
    }
    
    public static List<RestaurantPermission> ToEntityList(this IEnumerable<RestaurantPermissionDto> dtos)
    {
        return dtos.Select(d => d.ToEntity()).ToList();
    }
    
    public static RestaurantPermission ToEntity(this CreateRestaurantPermissionDto dto)
    {
        return new RestaurantPermission
        {
            RestaurantEmployeeId = dto.RestaurantEmployeeId,
            Permission = dto.Permission.ToDomain()
        };
    }
}