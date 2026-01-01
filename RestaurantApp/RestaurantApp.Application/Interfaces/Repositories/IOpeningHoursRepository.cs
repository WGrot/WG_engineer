using RestaurantApp.Domain.Models;

namespace RestaurantApp.Application.Interfaces.Repositories;

public interface IOpeningHoursRepository
{
    Task<IEnumerable<OpeningHours>> GetByRestaurantIdAsync(int restaurantId, CancellationToken ct);
    Task AddRangeAsync(IEnumerable<OpeningHours> openingHours, CancellationToken ct);
    void RemoveRange(IEnumerable<OpeningHours> openingHours, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
    
    Task<OpeningHours?> GetByRestaurantAndDayAsync(int restaurantId, DayOfWeek dayOfWeek, CancellationToken ct);
}