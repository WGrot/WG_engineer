using RestaurantApp.Domain.Models;
using RestaurantApp.Shared.DTOs.Menu.Categories;
using RestaurantApp.Shared.DTOs.Menu.MenuItems;

namespace RestaurantApp.Api.Mappers;

public static class MenuCategoryMapper
{
    // Z Entity na DTO
    public static MenuCategoryDto ToDto(this MenuCategory entity)
    {
        return new MenuCategoryDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Description = entity.Description,
            DisplayOrder = entity.DisplayOrder,
            IsActive = entity.IsActive,
            Items = entity.Items?.Select(i => i.ToDto()).ToList() ?? new List<MenuItemDto>()
            // MenuId i Menu nie są mapowane do DTO
        };
    }

    // Z DTO na Entity
    public static MenuCategory ToEntity(this MenuCategoryDto dto)
    {
        return new MenuCategory
        {
            Id = dto.Id,
            Name = dto.Name,
            Description = dto.Description,
            DisplayOrder = dto.DisplayOrder,
            IsActive = dto.IsActive,
            Items = dto.Items?.Select(i => i.ToEntity()).ToList() ?? new List<MenuItem>()
            // MenuId będzie ustawiony osobno
            // Menu będzie zarządzany przez EF
        };
    }

    // Aktualizacja istniejącej encji z DTO
    public static void UpdateFromDto(this MenuCategory entity, MenuCategoryDto dto)
    {
        entity.Name = dto.Name;
        entity.Description = dto.Description;
        entity.IsActive = dto.IsActive;
        
        // Aktualizacja pozycji menu w kategorii (jeśli przekazane)
        if (dto.Items != null)
        {
            entity.Items = dto.Items.Select(i => i.ToEntity()).ToList();
        }
        
        if (dto.DisplayOrder > 0)
        {
            entity.DisplayOrder = dto.DisplayOrder;
        }
        
        // Nie aktualizujemy Id, MenuId ani relacji Menu
    }

    // Mapowanie kolekcji Entity na listę DTO
    public static List<MenuCategoryDto> ToDtoList(this IEnumerable<MenuCategory> entities)
    {
        return entities.Select(e => e.ToDto()).ToList();
    }

    // Mapowanie kolekcji DTO na listę Entity
    public static List<MenuCategory> ToEntityList(this IEnumerable<MenuCategoryDto> dtos)
    {
        return dtos.Select(d => d.ToEntity()).ToList();
    }
}