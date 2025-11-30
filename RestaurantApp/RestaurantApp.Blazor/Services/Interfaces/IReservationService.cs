using RestaurantApp.Shared.DTOs.Reservation;
using RestaurantApp.Shared.Models;

namespace RestaurantApp.Blazor.Services.Interfaces;

public interface IReservationService
{
    public Task<(bool Success, string? Error)> DeleteReservationAsync(ReservationDto reservation);

    public Task<(bool Success, string? Error)> UpdateReservationStatusAsync(ReservationDto reservation, ReservationStatusEnumDto newStatusEnumDto);
}