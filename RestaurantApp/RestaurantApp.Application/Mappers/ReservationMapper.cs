using RestaurantApp.Application.Mappers.EnumMappers;
using RestaurantApp.Domain.Models;
using RestaurantApp.Shared.DTOs;
using RestaurantApp.Shared.DTOs.Reservation;

namespace RestaurantApp.Application.Mappers;

public static class ReservationMapper
{

    public static ReservationDto ToDto(this ReservationBase entity)
    {
        return new ReservationDto
        {
            Id = entity.Id,
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
            StatusEnumDto = entity.Status.ToShared(),
        };
    }
    
    public static ReservationBase ToEntity(this ReservationDto dto)
    {
        return new ReservationBase
        {
            Id = dto.Id,
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
            Status = dto.StatusEnumDto.ToDomain(),

        };
    }
    
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
        entity.Status = dto.StatusEnumDto.ToDomain();
    }
    
    public static List<ReservationDto> ToDtoList(this IEnumerable<ReservationBase> entities)
    {
        return entities.Select(e => e.ToDto()).ToList();
    }
    
    public static List<ReservationBase> ToEntityList(this IEnumerable<ReservationDto> dtos)
    {
        return dtos.Select(d => d.ToEntity()).ToList();
    }
    
    public static TableReservationDto ToTableReservationDto(this TableReservation reservation)
    {
        return new TableReservationDto
        {
            Id = reservation.Id,
            ReservationDate = reservation.ReservationDate,
            StartTime = reservation.StartTime,
            EndTime = reservation.EndTime,
            NumberOfGuests = reservation.NumberOfGuests,
            CustomerName = reservation.CustomerName,
            CustomerEmail = reservation.CustomerEmail,
            CustomerPhone = reservation.CustomerPhone,
            StatusEnumDto = reservation.Status.ToShared(),
            Notes = reservation.Notes,
            UserId = reservation.UserId,
            RestaurantId = reservation.RestaurantId,
            RestaurantName = reservation.Restaurant?.Name ?? string.Empty,
            RestaurantAddress = reservation.Restaurant?.Address ?? string.Empty,
            
            TableId = reservation.TableId,
            Table = reservation.Table?.ToDto() 
        };
    }
}