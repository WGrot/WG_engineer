using RestaurantApp.Shared.Models;

namespace RestaurantApp.Blazor.Services.Interfaces;

public interface IReservationService
{
    public Task<(bool Success, string? Error)> DeleteReservationAsync(ReservationBase reservation);

    public Task<(bool Success, string? Error)> UpdateReservationStatusAsync(ReservationBase reservation, ReservationStatus newStatus);
}