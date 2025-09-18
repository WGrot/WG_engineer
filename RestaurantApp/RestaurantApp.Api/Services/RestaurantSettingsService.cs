using Microsoft.EntityFrameworkCore;
using RestaurantApp.Api.Services.Interfaces;
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

    public async Task<IEnumerable<RestaurantSettings>> GetAllAsync()
    {
        try
        {
            return await _context.RestaurantSettings.ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting all restaurant settings");
            throw;
        }
    }

    public async Task<RestaurantSettings?> GetByIdAsync(int id)
    {
        try
        {
            return await _context.RestaurantSettings.FindAsync(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting restaurant settings with id {Id}", id);
            throw;
        }
    }

    public async Task<RestaurantSettings> CreateAsync(RestaurantSettings restaurantSettings)
    {
        try
        {
            _context.RestaurantSettings.Add(restaurantSettings);
            await _context.SaveChangesAsync();
            return restaurantSettings;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating restaurant settings");
            throw;
        }
    }

    public async Task<RestaurantSettings?> UpdateAsync(int id, RestaurantSettings restaurantSettings)
    {
        try
        {
            var existingSettings = await _context.RestaurantSettings.FindAsync(id);
            if (existingSettings == null)
            {
                return null;
            }

            existingSettings.ReservationsNeedConfirmation = restaurantSettings.ReservationsNeedConfirmation;

            _context.RestaurantSettings.Update(existingSettings);
            await _context.SaveChangesAsync();
            return existingSettings;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating restaurant settings with id {Id}", id);
            throw;
        }
    }

    public async Task<bool> DeleteAsync(int id)
    {
        try
        {
            var restaurantSettings = await _context.RestaurantSettings.FindAsync(id);
            if (restaurantSettings == null)
            {
                return false;
            }

            _context.RestaurantSettings.Remove(restaurantSettings);
            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting restaurant settings with id {Id}", id);
            throw;
        }
    }

    public async Task<bool> ExistsAsync(int id)
    {
        try
        {
            return await _context.RestaurantSettings.AnyAsync(rs => rs.Id == id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while checking if restaurant settings exists with id {Id}", id);
            throw;
        }
    }
}