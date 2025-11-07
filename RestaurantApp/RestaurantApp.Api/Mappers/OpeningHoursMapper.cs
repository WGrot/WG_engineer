using RestaurantApp.Domain.Models;
using RestaurantApp.Shared.DTOs;
using RestaurantApp.Shared.DTOs.OpeningHours;

namespace RestaurantApp.Api.Mappers;

public static class OpeningHoursMapper
{
    // Z Entity na DTO
    public static OpeningHoursDto ToDto(this OpeningHours entity)
    {
        return new OpeningHoursDto
        {
            DayOfWeek = (int)entity.DayOfWeek,
            OpenTime = entity.OpenTime.ToString("HH:mm"),
            CloseTime = entity.CloseTime.ToString("HH:mm"),
            IsClosed = entity.IsClosed
        };
    }

    // Z DTO na Entity
    public static OpeningHours ToEntity(this OpeningHoursDto dto)
    {
        return new OpeningHours
        {
            DayOfWeek = (DayOfWeek)dto.DayOfWeek,
            OpenTime = TimeOnly.Parse(dto.OpenTime),
            CloseTime = TimeOnly.Parse(dto.CloseTime),
            IsClosed = dto.IsClosed
            // Id i RestaurantId będą ustawione przez EF lub w warstwie serwisu
            // Restaurant nie jest mapowany - zarządzany przez EF
        };
    }

    // Aktualizacja istniejącej encji z DTO
    public static void UpdateFromDto(this OpeningHours entity, OpeningHoursDto dto)
    {
        entity.DayOfWeek = (DayOfWeek)dto.DayOfWeek;
        entity.OpenTime = TimeOnly.Parse(dto.OpenTime);
        entity.CloseTime = TimeOnly.Parse(dto.CloseTime);
        entity.IsClosed = dto.IsClosed;
        // Nie aktualizujemy Id, RestaurantId ani Restaurant
    }

    // Mapowanie kolekcji Entity na listę DTO
    public static List<OpeningHoursDto> ToDtoList(this IEnumerable<OpeningHours> entities)
    {
        return entities.Select(e => e.ToDto()).ToList();
    }

    // Mapowanie kolekcji DTO na listę Entity
    public static List<OpeningHours> ToEntityList(this IEnumerable<OpeningHoursDto> dtos)
    {
        return dtos.Select(d => d.ToEntity()).ToList();
    }

    // Pomocnicza metoda do bezpiecznego parsowania czasu
    public static TimeOnly? TryParseTime(string time)
    {
        if (TimeOnly.TryParse(time, out var result))
        {
            return result;
        }
        return null;
    }

    // Z DTO na Entity z walidacją
    public static OpeningHours? ToEntityWithValidation(this OpeningHoursDto dto)
    {
        var openTime = TryParseTime(dto.OpenTime);
        var closeTime = TryParseTime(dto.CloseTime);

        if (!openTime.HasValue || !closeTime.HasValue)
        {
            return null; // Niepoprawny format czasu
        }

        if (dto.DayOfWeek < 0 || dto.DayOfWeek > 6)
        {
            return null; // Niepoprawny dzień tygodnia
        }

        return new OpeningHours
        {
            DayOfWeek = (DayOfWeek)dto.DayOfWeek,
            OpenTime = openTime.Value,
            CloseTime = closeTime.Value,
            IsClosed = dto.IsClosed
        };
    }
}