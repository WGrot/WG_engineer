using RestaurantApp.Domain.Models;
using RestaurantApp.Shared.DTOs.OpeningHours;

namespace RestaurantApp.Application.Mappers;

public static class OpeningHoursMapper
{

    public static OpeningHoursDto ToDto(this OpeningHours entity)
    {
        return new OpeningHoursDto
        {
            DayOfWeek = entity.DayOfWeek,
            OpenTime = entity.OpenTime,
            CloseTime = entity.CloseTime,
            IsClosed = entity.IsClosed
        };
    }


    public static OpeningHours ToEntity(this OpeningHoursDto dto)
    {
        return new OpeningHours
        {
            DayOfWeek = dto.DayOfWeek,
            OpenTime = dto.OpenTime,
            CloseTime = dto.CloseTime,
            IsClosed = dto.IsClosed
        };
    }
    
    public static void UpdateFromDto(this OpeningHours entity, OpeningHoursDto dto)
    {
        entity.DayOfWeek = dto.DayOfWeek;
        entity.OpenTime = dto.OpenTime;
        entity.CloseTime = dto.CloseTime;
        entity.IsClosed = dto.IsClosed;
    }
    
    public static List<OpeningHoursDto> ToDtoList(this IEnumerable<OpeningHours> entities)
    {
        return entities.Select(e => e.ToDto()).ToList();
    }
    
    public static List<OpeningHours> ToEntityList(this IEnumerable<OpeningHoursDto> dtos)
    {
        return dtos.Select(d => d.ToEntity()).ToList();
    }
    
    public static TimeOnly? TryParseTime(string time)
    {
        if (TimeOnly.TryParse(time, out var result))
        {
            return result;
        }
        return null;
    }
    
    public static OpeningHours? ToEntityWithValidation(this OpeningHoursDto dto)
    {
        var openTime = dto.OpenTime;
        var closeTime = dto.CloseTime;
        

        if (dto.DayOfWeek < 0)
        {
            return null; 
        }

        return new OpeningHours
        {
            DayOfWeek = dto.DayOfWeek,
            OpenTime = openTime,
            CloseTime = closeTime,
            IsClosed = dto.IsClosed
        };
    }
}