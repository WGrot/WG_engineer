using RestaurantApp.Domain.Models;
using RestaurantApp.Shared.DTOs.Menu.Variants;

namespace RestaurantApp.Application.Mappers;

public static class MenuItemVariantMapper
{

    public static MenuItemVariantDto ToDto(this MenuItemVariant entity)
    {
        return new MenuItemVariantDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Price = entity.Price,
            IsAvailable = entity.IsAvailable,
            MenuItemId = entity.MenuItemId,
            Description = entity.Description
        };
    }


    public static MenuItemVariant ToEntity(this MenuItemVariantDto dto)
    {
        return new MenuItemVariant
        {
            Id = dto.Id,
            Name = dto.Name,
            Price = dto.Price,
            IsAvailable = dto.IsAvailable,
            MenuItemId = dto.MenuItemId,
            Description = dto.Description
        };
    }

    public static void UpdateFromDto(this MenuItemVariant entity, MenuItemVariantDto dto)
    {
        entity.Name = dto.Name;
        entity.Price = dto.Price;
        entity.IsAvailable = dto.IsAvailable;
        entity.MenuItemId = dto.MenuItemId;
        entity.Description = dto.Description;
    }
    
    public static List<MenuItemVariantDto> ToDtoList(this IEnumerable<MenuItemVariant> entities)
    {
        return entities.Select(e => e.ToDto()).ToList();
    }
}