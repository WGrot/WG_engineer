using RestaurantApp.Domain.Models;

namespace RestaurantApp.Application.Interfaces.Repositories;

public interface IOpeningHoursRepository
{
    Task<IEnumerable<OpeningHours>> GetByRestaurantIdAsync(int restaurantId);
    Task AddRangeAsync(IEnumerable<OpeningHours> openingHours);
    void RemoveRange(IEnumerable<OpeningHours> openingHours);
    Task SaveChangesAsync();
}