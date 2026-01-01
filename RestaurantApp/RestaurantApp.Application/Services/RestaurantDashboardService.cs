using RestaurantApp.Application.Interfaces.Repositories;
using RestaurantApp.Application.Interfaces.Services;
using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.DTOs.Restaurant;

namespace RestaurantApp.Application.Services;

public class RestaurantDashboardService : IRestaurantDashboardService
{
    private readonly IReservationRepository _reservationRepository;

    public RestaurantDashboardService(IReservationRepository reservationRepository)
    {
        _reservationRepository = reservationRepository;
    }

    public async Task<Result<RestaurantDashboardDataDto>> GetDashboardDataAsync(int restaurantId, CancellationToken ct)
    {
        var today = DateTime.SpecifyKind(DateTime.UtcNow.Date, DateTimeKind.Utc);
        var tomorrow = DateTime.SpecifyKind(today.AddDays(1), DateTimeKind.Utc);
        var lastWeek = DateTime.SpecifyKind(today.AddDays(-7), DateTimeKind.Utc);

        var todayReservationsCount = await _reservationRepository
            .CountByRestaurantAndDateRangeAsync(restaurantId, today, tomorrow, ct);

        var lastWeekReservationsCount = await _reservationRepository
            .CountByRestaurantAndDateRangeAsync(restaurantId, lastWeek, tomorrow, ct);

        var dto = new RestaurantDashboardDataDto
        {
            TodayReservations = todayReservationsCount,
            ReservationsLastWeek = lastWeekReservationsCount
        };

        return Result<RestaurantDashboardDataDto>.Success(dto);
    }
}