using RestaurantApp.Domain.Enums;
using RestaurantApp.Domain.Models;

namespace RestaurantApp.Application.Interfaces.Repositories;

public interface IImageLinkRepository
{
    Task<ImageLink?> GetByIdAsync(int id);
    Task<IEnumerable<ImageLink>> GetByRestaurantAndTypeAsync(int restaurantId, ImageType type);
    Task<IEnumerable<ImageLink>> GetByRestaurantIdAsync(int restaurantId);
    Task<int> GetMaxSortOrderAsync(int restaurantId, ImageType type);
    Task AddAsync(ImageLink image);
    Task Remove(ImageLink image);
    Task SaveChangesAsync();
}