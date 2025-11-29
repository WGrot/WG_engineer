using Microsoft.EntityFrameworkCore;
using RestaurantApp.Application.Interfaces.Repositories;
using RestaurantApp.Domain.Models;

namespace RestaurantApp.Infrastructure.Persistence.Repositories;

public class RestaurantRepository : IRestaurantRepository
{
    private readonly ApplicationDbContext _context;

    public RestaurantRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Restaurant?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return await _context.Restaurants.FindAsync([id], ct);
    }


    public async Task<bool> ExistsAsync(int restaurantId, CancellationToken ct = default)
    {
        return await _context.Restaurants.AnyAsync(r => r.Id == restaurantId, ct);
    }

    public async Task SaveChangesAsync(CancellationToken ct = default)
    {
        await _context.SaveChangesAsync(ct);
    }
}