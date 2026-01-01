using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.DTOs.Tables;

namespace RestaurantApp.Application.Interfaces.Services;

public interface ITableAvailabilityService
{
    Task<Result<TableAvailabilityResultDto>> CheckTableAvailabilityAsync(
        int tableId,
        DateTime date,
        TimeOnly startTime,
        TimeOnly endTime
        , CancellationToken ct = default);

    Task<Result<TableAvailability>> GetTableAvailabilityMapAsync(int tableId, DateTime date, CancellationToken ct = default);
}