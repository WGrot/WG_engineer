using Microsoft.EntityFrameworkCore;
using RestaurantApp.Application.Interfaces.Repositories;
using RestaurantApp.Domain.Models;

namespace RestaurantApp.Infrastructure.Persistence.Repositories;

public class TableRepository : ITableRepository
{
    private readonly ApplicationDbContext _context;

    public TableRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Table>> GetByRestaurantIdWithSeatsAsync(int restaurantId)
    {
        return await _context.Tables
            .Where(t => t.RestaurantId == restaurantId)
            .Include(t => t.Seats)
            .ToListAsync();
    }
}