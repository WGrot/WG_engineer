using Microsoft.EntityFrameworkCore;
using RestaurantApp.Application.Interfaces.Repositories;

namespace RestaurantApp.Infrastructure.Persistence.Repositories;

public class RestaurantRepository : IRestaurantRepository
{
    private readonly ApplicationDbContext _context;

    public RestaurantRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> ExistsAsync(int restaurantId, CancellationToken ct = default)
    {
        return await _context.Restaurants.AnyAsync(r => r.Id == restaurantId, ct);
    }
}