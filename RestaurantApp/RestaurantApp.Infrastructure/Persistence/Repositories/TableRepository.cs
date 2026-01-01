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

    public async Task<IEnumerable<Table>> GetAllWithRestaurantAndSeatsAsync(CancellationToken ct)
    {
        return await _context.Tables
            .Include(t => t.Restaurant)
            .ToListAsync(cancellationToken: ct);
    }

    public async Task<Table?> GetByIdWithRestaurantAndSeatsAsync(int id, CancellationToken ct)
    {
        return await _context.Tables
            .Include(t => t.Restaurant)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken: ct);
    }

    public async Task<IEnumerable<Table>> GetByRestaurantIdWithSeatsAsync(int restaurantId, CancellationToken ct)
    {
        return await _context.Tables
            .Where(t => t.RestaurantId == restaurantId)
            .ToListAsync(cancellationToken: ct);
    }

    public async Task<IEnumerable<Table>> GetAvailableTablesAsync(int? minCapacity, CancellationToken ct)
    {
        var query = _context.Tables
            .Include(t => t.Restaurant)
            .AsQueryable();

        if (minCapacity.HasValue)
        {
            query = query.Where(t => t.Capacity >= minCapacity.Value);
        }

        return await query.ToListAsync(cancellationToken: ct);
    }

    public async Task<Table?> GetByIdAsync(int id, CancellationToken ct)
    {
        return await _context.Tables.FindAsync(id, ct);
    }

    public async Task<bool> ExistsAsync(int id, CancellationToken ct)
    {
        return await _context.Tables.AnyAsync(t => t.Id == id, cancellationToken: ct);
    }

    public async Task<bool> TableNumberExistsInRestaurantAsync(
        string tableNumber, 
        int restaurantId, CancellationToken ct, 
        int? excludeTableId = null)
    {
        var query = _context.Tables
            .Where(t => t.TableNumber == tableNumber && t.RestaurantId == restaurantId);

        if (excludeTableId.HasValue)
        {
            query = query.Where(t => t.Id != excludeTableId.Value);
        }

        return await query.AnyAsync(cancellationToken: ct);
    }

    public async Task<Table> AddAsync(Table table, CancellationToken ct)
    {
        _context.Tables.Add(table);
        await _context.SaveChangesAsync(ct);
        
        return await GetByIdWithRestaurantAndSeatsAsync(table.Id, ct) 
               ?? throw new InvalidOperationException("Failed to retrieve created table.");
    }

    public async Task UpdateAsync(Table table, CancellationToken ct)
    {
        _context.Tables.Update(table);
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Table table, CancellationToken ct)
    {
        _context.Tables.Remove(table);
        await _context.SaveChangesAsync(ct);
    }
}