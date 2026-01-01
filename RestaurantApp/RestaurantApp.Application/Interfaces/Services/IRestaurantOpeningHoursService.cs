using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.DTOs.OpeningHours;

namespace RestaurantApp.Application.Interfaces.Services;

public interface IRestaurantOpeningHoursService
{
    Task<Result<OpenStatusDto>> CheckIfOpenAsync(int restaurantId,CancellationToken ct = default, TimeOnly? time = null, DayOfWeek? dayOfWeek = null);
    Task<Result> UpdateOpeningHoursAsync(int id, List<OpeningHoursDto> openingHours, CancellationToken ct = default);
}