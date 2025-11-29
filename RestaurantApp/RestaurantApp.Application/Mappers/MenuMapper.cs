using RestaurantApp.Domain.Models;
using RestaurantApp.Shared.DTOs.Menu;
using RestaurantApp.Shared.DTOs.Menu.Categories;

namespace RestaurantApp.Application.Mappers;


public static class MenuMapper
{
    // Z Entity na DTO
    public static MenuDto ToDto(this Menu entity)
    {
        return new MenuDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Description = entity.Description,
            IsActive = entity.IsActive,
            Categories = entity.Categories?.Select(c => c.ToDto()).ToList() ?? new List<MenuCategoryDto>(),
            Items = entity.Items?.Select(i => i.ToDto()).ToList()
            // RestaurantId i Restaurant nie są mapowane do DTO
        };
    }

    // Z DTO na Entity
    public static Menu ToEntity(this MenuDto dto)
    {
        return new Menu
        {
            Id = dto.Id,
            Name = dto.Name,
            Description = dto.Description,
            IsActive = dto.IsActive,
            Categories = dto.Categories?.Select(c => c.ToEntity()).ToList() ?? new List<MenuCategory>(),
            Items = dto.Items?.Select(i => i.ToEntity()).ToList()
            // RestaurantId będzie ustawiony osobno
            // Restaurant będzie zarządzany przez EF
        };
    }

    // Aktualizacja istniejącej encji z DTO
    public static void UpdateFromDto(this Menu entity, UpdateMenuDto dto)
    {
        entity.Name = dto.Name;
        entity.Description = dto.Description;
        entity.IsActive = dto.IsActive;
        
    }

    // Mapowanie kolekcji Entity na listę DTO
    public static List<MenuDto> ToDtoList(this IEnumerable<Menu> entities)
    {
        return entities.Select(e => e.ToDto()).ToList();
    }

    // Mapowanie kolekcji DTO na listę Entity
    public static List<Menu> ToEntityList(this IEnumerable<MenuDto> dtos)
    {
        return dtos.Select(d => d.ToEntity()).ToList();
    }
    
    public static Menu ToEntity(this CreateMenuDto dto)
    {
        return new Menu
        {
            RestaurantId = dto.RestaurantId,
            Name = dto.Name,
            Description = dto.Description,
            IsActive = dto.IsActive,
            Categories = new List<MenuCategory>(),
            Items = new List<MenuItem>()
        };
    }
}