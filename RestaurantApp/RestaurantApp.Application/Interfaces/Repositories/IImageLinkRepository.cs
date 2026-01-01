using RestaurantApp.Domain.Enums;
using RestaurantApp.Domain.Models;

namespace RestaurantApp.Application.Interfaces.Repositories;

public interface IImageLinkRepository
{
    Task<ImageLink?> GetByIdAsync(int id, CancellationToken ct);
    Task<IEnumerable<ImageLink>> GetByRestaurantAndTypeAsync(int restaurantId, ImageType type, CancellationToken ct);
    Task<IEnumerable<ImageLink>> GetByRestaurantIdAsync(int restaurantId, CancellationToken ct);
    Task<int> GetMaxSortOrderAsync(int restaurantId, ImageType type, CancellationToken ct);
    Task AddAsync(ImageLink image, CancellationToken ct);
    Task Remove(ImageLink image, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}