using RestaurantApp.Blazor.Extensions;
using RestaurantApp.Blazor.Services.Interfaces;
using RestaurantApp.Shared.DTOs.Reservation;
using RestaurantApp.Shared.Models;

namespace RestaurantApp.Blazor.Services;

public class ReservationService : IReservationService
{
    private readonly HttpClient _http;

    public ReservationService(HttpClient http)
    {
        _http = http;
    }

    public async Task<(bool Success, string? Error)> DeleteReservationAsync(ReservationDto reservation)
    {
        try
        {
            var response = await _http.RequestWithHeaderAsync(
                HttpMethod.Delete,
                $"api/reservation/{reservation.Id}",
                0,
                "X-Restaurant-Id",
                reservation.RestaurantId.ToString()
            );

            if (response.IsSuccessStatusCode)
                return (true, null);

            var msg = await response.Content.ReadAsStringAsync();
            return (false, $"Failed to delete reservation: {msg}");
        }
        catch (Exception ex)
        {
            return (false, $"Error deleting reservation: {ex.Message}");
        }
    }

    public async Task<(bool Success, string? Error)> UpdateReservationStatusAsync(ReservationDto reservation, ReservationStatusEnumDto newStatusEnumDto)
    {
        try
        {
            var response = await _http.RequestWithHeaderAsync(
                HttpMethod.Put,
                $"api/reservation/manage/{reservation.Id}/change-statusEnumDto",
                newStatusEnumDto,
                "X-Restaurant-Id",
                reservation.RestaurantId.ToString()
            );

            if (response.IsSuccessStatusCode)
                return (true, null);

            var msg = await response.Content.ReadAsStringAsync();
            return (false, $"Failed to update statusEnumDto: {msg}");
        }
        catch (Exception ex)
        {
            return (false, $"Error updating statusEnumDto: {ex.Message}");
        }
    }
}
