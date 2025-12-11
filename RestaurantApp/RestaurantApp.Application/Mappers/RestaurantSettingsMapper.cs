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
            ReservationsNeedConfirmation = entity.ReservationsNeedConfirmation,
            MaxReservationDuration = entity.MaxReservationDuration,
            MinReservationDuration = entity.MinReservationDuration,
            MaxAdvanceBookingTime =  entity.MaxAdvanceBookingTime,
            MinAdvanceBookingTime = entity.MinAdvanceBookingTime,
            MaxGuestsPerReservation = entity.MaxGuestsPerReservation,
            MinGuestsPerReservation = entity.MinGuestsPerReservation,
            ReservationsPerUserLimit = entity.ReservationsPerUserLimit

        };
    }


    public static RestaurantSettings ToEntity(this SettingsDto dto)
    {
        return new RestaurantSettings
        {
            Id = dto.Id,
            RestaurantId = dto.RestaurantId,
            ReservationsNeedConfirmation = dto.ReservationsNeedConfirmation,
            MaxReservationDuration = dto.MaxReservationDuration,
            MinReservationDuration = dto.MinReservationDuration,
            MaxAdvanceBookingTime = dto.MaxAdvanceBookingTime,
            MinAdvanceBookingTime = dto.MinAdvanceBookingTime,
            MaxGuestsPerReservation = dto.MaxGuestsPerReservation,
            MinGuestsPerReservation = dto.MinGuestsPerReservation,
            ReservationsPerUserLimit = dto.ReservationsPerUserLimit

        };
    }

    public static void UpdateFromDto(this RestaurantSettings entity, UpdateRestaurantSettingsDto dto)
    {
        entity.ReservationsNeedConfirmation = dto.ReservationsNeedConfirmation;
        entity.MaxReservationDuration = dto.MaxReservationDuration;
        entity.MinReservationDuration = dto.MinReservationDuration;
        entity.MaxAdvanceBookingTime = dto.MaxAdvanceBookingTime;
        entity.MinAdvanceBookingTime = dto.MinAdvanceBookingTime;
        entity.MaxGuestsPerReservation = dto.MaxGuestsPerReservation;
        entity.MinGuestsPerReservation = dto.MinGuestsPerReservation;
        entity.ReservationsPerUserLimit = dto.ReservationsPerUserLimit;
        
        
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