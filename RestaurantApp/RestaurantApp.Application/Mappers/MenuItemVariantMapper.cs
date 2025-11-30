using RestaurantApp.Domain.Models;
using RestaurantApp.Shared.DTOs.Menu.Variants;

namespace RestaurantApp.Application.Mappers;

public static class MenuItemVariantMapper
{
    // Z Entity na DTO
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

    // Z DTO na Entity
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

    // Aktualizacja istniejącej encji z DTO
    public static void UpdateFromDto(this MenuItemVariant entity, MenuItemVariantDto dto)
    {
        entity.Name = dto.Name;
        entity.Price = dto.Price;
        entity.IsAvailable = dto.IsAvailable;
        entity.MenuItemId = dto.MenuItemId;
        entity.Description = dto.Description;
        // Id zazwyczaj nie aktualizujemy
    }

    // Mapowanie kolekcji
    public static List<MenuItemVariantDto> ToDtoList(this IEnumerable<MenuItemVariant> entities)
    {
        return entities.Select(e => e.ToDto()).ToList();
    }
}