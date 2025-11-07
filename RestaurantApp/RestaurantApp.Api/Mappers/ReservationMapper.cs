using RestaurantApp.Domain.Models;
using RestaurantApp.Shared.DTOs;

namespace RestaurantApp.Api.Mappers;

public static class ReservationMapper
{
    // Z Entity na DTO
    public static ReservationDto ToDto(this ReservationBase entity)
    {
        return new ReservationDto
        {
            ReservationDate = entity.ReservationDate,
            StartTime = entity.StartTime,
            EndTime = entity.EndTime,
            NumberOfGuests = entity.NumberOfGuests,
            CustomerName = entity.CustomerName,
            CustomerEmail = entity.CustomerEmail,
            CustomerPhone = entity.CustomerPhone,
            Notes = entity.Notes,
            UserId = entity.UserId,
            requiresConfirmation = entity.NeedsConfirmation,
            RestaurantId = entity.RestaurantId,
            RestaurantName = entity.Restaurant?.Name ?? string.Empty,
            RestaurantAddress = entity.Restaurant?.Address ?? string.Empty,
            Status = entity.Status,
        };
    }

    // Z DTO na Entity
    public static ReservationBase ToEntity(this ReservationDto dto)
    {
        return new ReservationBase
        {
            ReservationDate = dto.ReservationDate,
            StartTime = dto.StartTime,
            EndTime = dto.EndTime,
            NumberOfGuests = dto.NumberOfGuests,
            CustomerName = dto.CustomerName,
            CustomerEmail = dto.CustomerEmail,
            CustomerPhone = dto.CustomerPhone,
            Notes = dto.Notes,
            UserId = dto.UserId,
            NeedsConfirmation = dto.requiresConfirmation,
            RestaurantId = dto.RestaurantId,
            Status = dto.Status,
            // Id, Status, CreatedAt i Restaurant są ustawiane automatycznie lub przez EF
        };
    }

    // Aktualizacja istniejącej encji z DTO
    public static void UpdateFromDto(this ReservationBase entity, ReservationDto dto)
    {
        entity.ReservationDate = dto.ReservationDate;
        entity.StartTime = dto.StartTime;
        entity.EndTime = dto.EndTime;
        entity.NumberOfGuests = dto.NumberOfGuests;
        entity.CustomerName = dto.CustomerName;
        entity.CustomerEmail = dto.CustomerEmail;
        entity.CustomerPhone = dto.CustomerPhone;
        entity.Notes = dto.Notes;
        entity.UserId = dto.UserId;
        entity.NeedsConfirmation = dto.requiresConfirmation;
        entity.RestaurantId = dto.RestaurantId;
        entity.Status = dto.Status;
        // Id, Status, CreatedAt nie aktualizujemy
    }

    // Mapowanie kolekcji DTO na listę
    public static List<ReservationDto> ToDtoList(this IEnumerable<ReservationBase> entities)
    {
        return entities.Select(e => e.ToDto()).ToList();
    }

    // Mapowanie kolekcji Entity na listę (dodana metoda)
    public static List<ReservationBase> ToEntityList(this IEnumerable<ReservationDto> dtos)
    {
        return dtos.Select(d => d.ToEntity()).ToList();
    }
}