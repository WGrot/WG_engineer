using RestaurantApp.Domain.Models;
using RestaurantApp.Shared.DTOs;
using RestaurantApp.Shared.DTOs.Menu.MenuItems;

namespace RestaurantApp.Api.Mappers;

public static class MenuItemMapper
{
    public static MenuItemDto ToDto(this MenuItem entity)
    {
        return new MenuItemDto
        {
            Name = entity.Name,
            Description = entity.Description,
            Price = entity.Price.Price,
            CurrencyCode = entity.Price.CurrencyCode,
            ImagePath = entity.ImageUrl
        };
    }
    
    // IEnumerable<MenuItem> -> IEnumerable<MenuItemDto>
    public static IEnumerable<MenuItemDto> ToDto(this IEnumerable<MenuItem> entities)
    {
        return entities.Select(e => e.ToDto());
    }
    
    // MenuItemDto -> MenuItem
    public static MenuItem ToEntity(this MenuItemDto dto)
    {
        return new MenuItem
        {
            Name = dto.Name,
            Description = dto.Description,
            Price = new MenuItemPrice 
            { 
                Price = dto.Price, 
                CurrencyCode = dto.CurrencyCode 
            },
            ImageUrl = dto.ImagePath
        };
    }
    
    // Aktualizacja istniejącej encji z DTO
    public static void UpdateFromDto(this MenuItem entity, MenuItemDto dto)
    {
        entity.Name = dto.Name;
        entity.Description = dto.Description;
        entity.Price.Price = dto.Price;
        entity.Price.CurrencyCode = dto.CurrencyCode;
        entity.ImageUrl = dto.ImagePath;
    }
}