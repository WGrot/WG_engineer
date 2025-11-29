using RestaurantApp.Domain.Models;
using RestaurantApp.Shared.DTOs.Settings;

namespace RestaurantApp.Application.Mappers;

public static class RestaurantSettingsMapper
{
    public static SettingsDto ToDto(this RestaurantSettings entity)
    {
        return new SettingsDto
        {
            Id = entity.Id,
            RestaurantId = entity.RestaurantId,
            ReservationsNeedConfirmation = entity.ReservationsNeedConfirmation
            // Restaurant nie jest mapowana do DTO
        };
    }

    // Z DTO na Entity
    public static RestaurantSettings ToEntity(this SettingsDto dto)
    {
        return new RestaurantSettings
        {
            Id = dto.Id,
            RestaurantId = dto.RestaurantId,
            ReservationsNeedConfirmation = dto.ReservationsNeedConfirmation
            // Restaurant będzie zarządzany przez EF
        };
    }

    // Aktualizacja istniejącej encji z DTO
    public static void UpdateFromDto(this RestaurantSettings entity, SettingsDto dto)
    {
        entity.ReservationsNeedConfirmation = dto.ReservationsNeedConfirmation;
        
    }

    // Mapowanie kolekcji Entity na listę DTO
    public static List<SettingsDto> ToDtoList(this IEnumerable<RestaurantSettings> entities)
    {
        return entities.Select(e => e.ToDto()).ToList();
    }

    // Mapowanie kolekcji DTO na listę Entity
    public static List<RestaurantSettings> ToEntityList(this IEnumerable<SettingsDto> dtos)
    {
        return dtos.Select(d => d.ToEntity()).ToList();
    }
    
    // Opcjonalna metoda do tworzenia domyślnych ustawień dla restauracji
    public static RestaurantSettings CreateDefault(int restaurantId)
    {
        return new RestaurantSettings
        {
            RestaurantId = restaurantId,
            ReservationsNeedConfirmation = false
        };
    }
    
}