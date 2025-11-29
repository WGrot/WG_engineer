using Microsoft.EntityFrameworkCore;
using RestaurantApp.Api.Mappers;
using RestaurantApp.Api.Services.Interfaces;
using RestaurantApp.Domain.Models;
using RestaurantApp.Infrastructure.Persistence;
using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.DTOs;
using RestaurantApp.Shared.DTOs.Settings;
using RestaurantApp.Shared.Models;

namespace RestaurantApp.Api.Services;

public class RestaurantSettingsService : IRestaurantSettingsService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<RestaurantSettingsService> _logger;

    public RestaurantSettingsService(ApplicationDbContext context, ILogger<RestaurantSettingsService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<IEnumerable<SettingsDto>>> GetAllAsync()
    {
        var result = await _context.RestaurantSettings.ToListAsync();
        return Result<IEnumerable<SettingsDto>>.Success(result.ToDtoList());
    }

    public async Task<Result<SettingsDto>> GetByIdAsync(int id)
    {
        var result = await _context.RestaurantSettings.FindAsync(id);

        if (result == null)
        {
            return Result<SettingsDto>.NotFound($"Restaurant settings with id {id} not found");
        }

        return Result<SettingsDto>.Success(result.ToDto());
    }

    public async Task<Result<SettingsDto>> CreateAsync(CreateRestaurantSettingsDto restaurantSettingsDto)
    {
        
        RestaurantSettings restaurantSettings = new RestaurantSettings
        {
            RestaurantId = restaurantSettingsDto.RestaurantId,
            ReservationsNeedConfirmation = restaurantSettingsDto.ReservationsNeedConfirmation,
        };
        _context.RestaurantSettings.Add(restaurantSettings);
        await _context.SaveChangesAsync();
        return Result<SettingsDto>.Success(restaurantSettings.ToDto());
    }

    public async Task<Result<SettingsDto>> UpdateAsync(int id, UpdateRestaurantSettingsDto restaurantSettings)
    {
        var existingSettings = await _context.RestaurantSettings.FindAsync(id);
        if (existingSettings == null)
        {
            return Result<SettingsDto>.NotFound($"Restaurant settings with id {id} not found");
        }

        existingSettings.ReservationsNeedConfirmation = restaurantSettings.ReservationsNeedConfirmation;

        _context.RestaurantSettings.Update(existingSettings);
        await _context.SaveChangesAsync();
        return Result<SettingsDto>.Success(existingSettings.ToDto());
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

    public async Task<Result<SettingsDto>> GetByRestaurantId(int restaurantId)
    {
        var result = await _context.RestaurantSettings.Where(r =>r.RestaurantId == restaurantId)
            .FirstOrDefaultAsync();
        if (result == null)
        {
            return Result<SettingsDto>.NotFound($"Restaurant settings with id {restaurantId} not found");
        }
        return Result<SettingsDto>.Success(result.ToDto());
    }
}