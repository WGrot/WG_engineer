using Microsoft.EntityFrameworkCore;
using RestaurantApp.Api.Common;
using RestaurantApp.Api.Models.DTOs;
using RestaurantApp.Api.Services.Interfaces;
using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.Models;

namespace RestaurantApp.Api.Services;

public class TableService : ITableService
{
    private readonly ApiDbContext _context;

    public TableService(ApiDbContext context)
    {
        _context = context;
    }

    public async Task<Result<IEnumerable<Table>>> GetTablesAsync()
    {
        var tables = await _context.Tables
            .Include(t => t.Restaurant)
            .Include(t => t.Seats)
            .ToListAsync();

        return Result.Success<IEnumerable<Table>>(tables);
    }

    public async Task<Result<Table>> GetTableByIdAsync(int id)
    {
        var table = await _context.Tables
            .Include(t => t.Restaurant)
            .Include(t => t.Seats)
            .FirstOrDefaultAsync(t => t.Id == id);

        return table is null
            ? Result<Table>.NotFound($"Table with ID {id} not found.")
            : Result.Success(table);
    }

    public async Task<Result<IEnumerable<Table>>> GetTablesByRestaurantAsync(int restaurantId)
    {
        var tables = await _context.Tables
            .Where(t => t.RestaurantId == restaurantId)
            .Include(t => t.Seats)
            .ToListAsync();

        return Result.Success<IEnumerable<Table>>(tables);
    }

    public async Task<Result<IEnumerable<Table>>> GetAvailableTablesAsync(int? minCapacity)
    {
        var query = _context.Tables
            .Include(t => t.Restaurant)
            .Include(t => t.Seats)
            .AsQueryable();

        if (minCapacity.HasValue)
        {
            query = query.Where(t => t.Capacity >= minCapacity.Value);
        }

        var tables = await query.ToListAsync();
        return Result.Success<IEnumerable<Table>>(tables);
    }

    public async Task<Result<Table>> CreateTableAsync(CreateTableDto dto)
    {
        var restaurantExists = await _context.Restaurants.AnyAsync(r => r.Id == dto.RestaurantId);
        if (!restaurantExists)
            return Result<Table>.ValidationError($"Restaurant with ID {dto.RestaurantId} does not exist.");

        var tableNumberExists = await _context.Tables
            .AnyAsync(t => t.TableNumber == dto.TableNumber && t.RestaurantId == dto.RestaurantId);

        if (tableNumberExists)
            return Result<Table>.Conflict($"Table number {dto.TableNumber} already exists in this restaurant.");

        var table = new Table
        {
            TableNumber = dto.TableNumber,
            Capacity = dto.Capacity,
            Location = dto.Location,
            RestaurantId = dto.RestaurantId,
            Seats = new List<Seat>()
        };

        if (dto.SeatCount > 0)
        {
            for (int i = 1; i <= dto.SeatCount; i++)
            {
                table.Seats.Add(new Seat
                {
                    SeatNumber = $"{table.TableNumber}-{i}",
                    IsAvailable = true,
                    Type = "Standard"
                });
            }
        }

        _context.Tables.Add(table);
        await _context.SaveChangesAsync();

        var createdTable = await _context.Tables
            .Include(t => t.Restaurant)
            .Include(t => t.Seats)
            .FirstAsync(t => t.Id == table.Id);

        return Result<Table>.Created(createdTable);
    }

    public async Task<Result> UpdateTableAsync(int id, UpdateTableDto dto)
    {
        var table = await _context.Tables.FindAsync(id);
        if (table is null)
            return Result.NotFound($"Table with ID {id} not found.");

        if (table.TableNumber != dto.TableNumber)
        {
            var exists = await _context.Tables.AnyAsync(t =>
                t.TableNumber == dto.TableNumber &&
                t.RestaurantId == table.RestaurantId &&
                t.Id != id);

            if (exists)
                return Result.Conflict($"Table number {dto.TableNumber} already exists in this restaurant.");
        }

        table.TableNumber = dto.TableNumber;
        table.Capacity = dto.Capacity;
        table.Location = dto.Location;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            return Result.InternalError("Concurrency error while updating table.");
        }

        return Result.Success();
    }

    public async Task<Result> UpdateTableCapacityAsync(int id, int capacity)
    {
        if (capacity <= 0)
            return Result.ValidationError("Capacity must be greater than 0.");

        var table = await _context.Tables.FindAsync(id);
        if (table is null)
            return Result.NotFound($"Table with ID {id} not found.");

        table.Capacity = capacity;
        await _context.SaveChangesAsync();

        return Result.Success();
    }

    public async Task<Result> DeleteTableAsync(int id)
    {
        var table = await _context.Tables
            .Include(t => t.Seats)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (table is null)
            return Result.NotFound($"Table with ID {id} not found.");

        _context.Tables.Remove(table);
        await _context.SaveChangesAsync();

        return Result.Success();
    }
}