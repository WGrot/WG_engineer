using RestaurantApp.Domain.Models;
using RestaurantApp.Shared.DTOs.Menu.Categories;
using RestaurantApp.Shared.DTOs.Menu.MenuItems;

namespace RestaurantApp.Application.Mappers;

public static class MenuCategoryMapper
{
    // Z Entity na DTO
    public static MenuCategoryDto ToDto(this MenuCategory entity)
    {
        return new MenuCategoryDto
        {
            Id = entity.Id,
            MenuId = entity.MenuId,
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
            MenuId = dto.MenuId,
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
    public static void UpdateFromDto(this MenuCategory entity, UpdateMenuCategoryDto dto)
    {
        if (!string.IsNullOrWhiteSpace(dto.Name))
        {
            entity.Name = dto.Name;
        }

        if (!string.IsNullOrWhiteSpace(dto.Description))
        {
            entity.Description = dto.Description;
        }

        if (dto.IsActive != null)
        {
            entity.IsActive = dto.IsActive.Value;
        }

        
        // Aktualizacja pozycji menu w kategorii (jeśli przekazane)
        if (dto.Items != null)
        {
            entity.Items = dto.Items.Select(i => i.ToEntity()).ToList();
        }
        
        if (dto.DisplayOrder > 0)
        {
            entity.DisplayOrder = dto.DisplayOrder;
        }
        
    }
    
    // NOWA METODA: Z CreateMenuCategoryDto na Entity
    public static MenuCategory ToEntity(this CreateMenuCategoryDto dto)
    {
        return new MenuCategory
        {
            MenuId = dto.MenuId,
            Name = dto.Name,
            Description = dto.Description,
            DisplayOrder = dto.DisplayOrder,
            IsActive = dto.IsActive,
            Items = new List<MenuItem>() // Nowa kategoria nie ma jeszcze pozycji
        };
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