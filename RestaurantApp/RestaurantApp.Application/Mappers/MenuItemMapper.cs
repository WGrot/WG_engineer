using RestaurantApp.Domain.Models;
using RestaurantApp.Shared.DTOs;
using RestaurantApp.Shared.DTOs.Menu.MenuItems;
using RestaurantApp.Shared.DTOs.Menu.Tags;

namespace RestaurantApp.Application.Mappers;

public static class MenuItemMapper
{
    public static MenuItemDto ToDto(this MenuItem entity)
    {
        return new MenuItemDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Description = entity.Description,
            Price = entity.Price.ToDto(),
            CurrencyCode = entity.Price.CurrencyCode,
            ImageUrl = entity.ImageUrl,
            ThumbnailUrl = entity.ThumbnailUrl,
            CategoryId = entity.CategoryId,

            Tags = entity.Tags?.ToDto().ToList() ?? new List<MenuItemTagDto>()
        };
    }


    public static IEnumerable<MenuItemDto> ToDtoList(this IEnumerable<MenuItem> entities)
    {
        return entities.Select(e => e.ToDto());
    }

    public static MenuItem ToEntity(this MenuItemDto dto)
    {
        var menuItem = new MenuItem
        {
            Id = dto.Id,
            Name = dto.Name,
            Description = dto.Description,
            Price = dto.Price.ToEntity(),
            ImageUrl = dto.ImageUrl,
            ThumbnailUrl = dto.ThumbnailUrl,
            CategoryId = dto.CategoryId
        };
        
        if (dto.Tags?.Any() == true)
        {
            menuItem.Tags = dto.Tags.Select(t => t.ToEntity()).ToHashSet();
        }

        return menuItem;
    }
    
    public static void UpdateFromDto(this MenuItem entity, MenuItemDto dto)
    {
        entity.Name = dto.Name;
        entity.Description = dto.Description;
        entity.Price = dto.Price.ToEntity();
        entity.Price.CurrencyCode = dto.CurrencyCode;
        entity.ImageUrl = dto.ImageUrl;
        entity.ThumbnailUrl = dto.ThumbnailUrl;
        
        entity.Tags.Clear();
        if (dto.Tags?.Any() == true)
        {
            foreach (var tagDto in dto.Tags)
            {
                entity.Tags.Add(tagDto.ToEntity());
            }
        }
    }
}