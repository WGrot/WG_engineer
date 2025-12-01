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

        };
    }


    public static RestaurantSettings ToEntity(this SettingsDto dto)
    {
        return new RestaurantSettings
        {
            Id = dto.Id,
            RestaurantId = dto.RestaurantId,
            ReservationsNeedConfirmation = dto.ReservationsNeedConfirmation

        };
    }

    public static void UpdateFromDto(this RestaurantSettings entity, SettingsDto dto)
    {
        entity.ReservationsNeedConfirmation = dto.ReservationsNeedConfirmation;
        
    }

    public static List<SettingsDto> ToDtoList(this IEnumerable<RestaurantSettings> entities)
    {
        return entities.Select(e => e.ToDto()).ToList();
    }

    public static List<RestaurantSettings> ToEntityList(this IEnumerable<SettingsDto> dtos)
    {
        return dtos.Select(d => d.ToEntity()).ToList();
    }
    

    public static RestaurantSettings CreateDefault(int restaurantId)
    {
        return new RestaurantSettings
        {
            RestaurantId = restaurantId,
            ReservationsNeedConfirmation = false
        };
    }
    
}