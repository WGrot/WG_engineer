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

    public async Task<IEnumerable<Table>> GetAllWithRestaurantAndSeatsAsync()
    {
        return await _context.Tables
            .Include(t => t.Restaurant)
            .ToListAsync();
    }

    public async Task<Table?> GetByIdWithRestaurantAndSeatsAsync(int id)
    {
        return await _context.Tables
            .Include(t => t.Restaurant)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<IEnumerable<Table>> GetByRestaurantIdWithSeatsAsync(int restaurantId)
    {
        return await _context.Tables
            .Where(t => t.RestaurantId == restaurantId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Table>> GetAvailableTablesAsync(int? minCapacity)
    {
        var query = _context.Tables
            .Include(t => t.Restaurant)
            .AsQueryable();

        if (minCapacity.HasValue)
        {
            query = query.Where(t => t.Capacity >= minCapacity.Value);
        }

        return await query.ToListAsync();
    }

    public async Task<Table?> GetByIdAsync(int id)
    {
        return await _context.Tables.FindAsync(id);
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Tables.AnyAsync(t => t.Id == id);
    }

    public async Task<bool> TableNumberExistsInRestaurantAsync(
        string tableNumber, 
        int restaurantId, 
        int? excludeTableId = null)
    {
        var query = _context.Tables
            .Where(t => t.TableNumber == tableNumber && t.RestaurantId == restaurantId);

        if (excludeTableId.HasValue)
        {
            query = query.Where(t => t.Id != excludeTableId.Value);
        }

        return await query.AnyAsync();
    }

    public async Task<Table> AddAsync(Table table)
    {
        _context.Tables.Add(table);
        await _context.SaveChangesAsync();
        
        return await GetByIdWithRestaurantAndSeatsAsync(table.Id) 
               ?? throw new InvalidOperationException("Failed to retrieve created table.");
    }

    public async Task UpdateAsync(Table table)
    {
        _context.Tables.Update(table);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Table table)
    {
        _context.Tables.Remove(table);
        await _context.SaveChangesAsync();
    }
}