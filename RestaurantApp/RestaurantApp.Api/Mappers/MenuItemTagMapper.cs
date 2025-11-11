using RestaurantApp.Domain.Models;
using RestaurantApp.Shared.DTOs;
using RestaurantApp.Shared.DTOs.Menu.Tags;

namespace RestaurantApp.Api.Mappers;

public static class MenuItemTagMapper
{
    // MenuItemTag -> MenuItemTagDto
    public static MenuItemTagDto ToDto(this MenuItemTag entity)
    {
        return new MenuItemTagDto
        {
            Id = entity.Id,
            Name = entity.Name,
            ColorHex = entity.ColorHex,
            RestaurantId = entity.RestaurantId
        };
    }
    
    // MenuItemTagDto -> MenuItemTag
    public static MenuItemTag ToEntity(this MenuItemTagDto dto)
    {
        return new MenuItemTag
        {
            Id = dto.Id,
            Name = dto.Name,
            ColorHex = dto.ColorHex,
            RestaurantId = dto.RestaurantId
        };
    }
    
    // Aktualizacja istniejącej encji z DTO
    public static void UpdateFromDto(this MenuItemTag entity, MenuItemTagDto dto)
    {
        entity.Id = dto.Id;
        entity.Name = dto.Name;
        entity.ColorHex = dto.ColorHex;
        entity.RestaurantId = dto.RestaurantId;
    }
    
    public static MenuItemTag ToEntity(this CreateMenuItemTagDto dto)
    {
        return new MenuItemTag
        {
            Id = dto.Id,
            Name = dto.Name,
            ColorHex = dto.ColorHex,
            RestaurantId = dto.RestaurantId
        };
    }
    
    public static IEnumerable<MenuItemTagDto> ToDto(this IEnumerable<MenuItemTag> entities)
    {
        return entities.Select(e => e.ToDto());
    }
}