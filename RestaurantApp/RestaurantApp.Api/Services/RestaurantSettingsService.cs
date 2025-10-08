using Microsoft.EntityFrameworkCore;
using RestaurantApp.Api.Services.Interfaces;
using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.DTOs;
using RestaurantApp.Shared.Models;

namespace RestaurantApp.Api.Services;

public class RestaurantSettingsService : IRestaurantSettingsService
{
    private readonly ApiDbContext _context;
    private readonly ILogger<RestaurantSettingsService> _logger;

    public RestaurantSettingsService(ApiDbContext context, ILogger<RestaurantSettingsService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<IEnumerable<RestaurantSettings>>> GetAllAsync()
    {
        var result = await _context.RestaurantSettings.ToListAsync();
        return Result<IEnumerable<RestaurantSettings>>.Success(result);
    }

    public async Task<Result<RestaurantSettings>> GetByIdAsync(int id)
    {
        var result = await _context.RestaurantSettings.FindAsync(id);

        if (result == null)
        {
            return Result<RestaurantSettings>.NotFound($"Restaurant settings with id {id} not found");
        }

        return Result<RestaurantSettings>.Success(result);
    }

    public async Task<Result<RestaurantSettings>> CreateAsync(RestaurantSettings restaurantSettings)
    {
        _context.RestaurantSettings.Add(restaurantSettings);
        await _context.SaveChangesAsync();
        return Result<RestaurantSettings>.Success(restaurantSettings);
    }

    public async Task<Result<RestaurantSettings>> UpdateAsync(int id, UpdateRestaurantSettingsDto restaurantSettings)
    {
        var existingSettings = await _context.RestaurantSettings.FindAsync(id);
        if (existingSettings == null)
        {
            return Result<RestaurantSettings>.NotFound($"Restaurant settings with id {id} not found");
        }

        existingSettings.ReservationsNeedConfirmation = restaurantSettings.ReservationsNeedConfirmation;

        _context.RestaurantSettings.Update(existingSettings);
        await _context.SaveChangesAsync();
        return Result<RestaurantSettings>.Success(existingSettings);
    }

    public async Task<Result> DeleteAsync(int id)
    {
        var restaurantSettings = await _context.RestaurantSettings.FindAsync(id);
        if (restaurantSettings == null)
        {
            return Result.NotFound($"Restaurant settings with id {id} not found");
        }

        _context.RestaurantSettings.Remove(restaurantSettings);
        await _context.SaveChangesAsync();
        return Result.Success();
    }

    public async Task<Result> ExistsAsync(int id)
    {
        var result = await _context.RestaurantSettings.AnyAsync(rs => rs.Id == id);
        if (result == false)
        {
            Result.NotFound($"Restaurant settings with id {id} not found");
        }
        return Result.Success(result);
    }

    public async Task<Result<bool>> NeedConfirmation(int restaurantId)
    {
        var result = await _context.RestaurantSettings.Where(r =>r.RestaurantId == restaurantId)
            .FirstOrDefaultAsync();

        if (result == null)
        {
            return Result<bool>.NotFound($"Restaurant settings with id {restaurantId} not found");
        }
        return Result<bool>.Success(result.ReservationsNeedConfirmation);
    }

    public async Task<Result<RestaurantSettings>> GetByRestaurantId(int restaurantId)
    {
        var result = await _context.RestaurantSettings.Where(r =>r.RestaurantId == restaurantId)
            .FirstOrDefaultAsync();
        if (result == null)
        {
            return Result<RestaurantSettings>.NotFound($"Restaurant settings with id {restaurantId} not found");
        }
        return Result<RestaurantSettings>.Success(result);
    }
}