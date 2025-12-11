using Microsoft.Extensions.Logging;
using RestaurantApp.Application.Interfaces.Repositories;
using RestaurantApp.Application.Interfaces.Services;
using RestaurantApp.Application.Mappers;
using RestaurantApp.Domain.Models;
using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.DTOs.Settings;

namespace RestaurantApp.Application.Services;

public class RestaurantSettingsService : IRestaurantSettingsService
{
    private readonly IRestaurantSettingsRepository _repository;

    public RestaurantSettingsService(IRestaurantSettingsRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<IEnumerable<SettingsDto>>> GetAllAsync()
    {
        var settings = await _repository.GetAllAsync();
        return Result<IEnumerable<SettingsDto>>.Success(settings.ToDtoList());
    }

    public async Task<Result<SettingsDto>> GetByIdAsync(int id)
    {
        var settings = await _repository.GetByIdAsync(id);

        if (settings is null)
            return Result<SettingsDto>.NotFound($"Restaurant settings with ID {id} not found.");

        return Result<SettingsDto>.Success(settings.ToDto());
    }

    public async Task<Result<SettingsDto>> GetByRestaurantIdAsync(int restaurantId)
    {
        var settings = await _repository.GetByRestaurantIdAsync(restaurantId);

        if (settings is null)
            return Result<SettingsDto>.NotFound($"Restaurant settings for restaurant {restaurantId} not found.");

        return Result<SettingsDto>.Success(settings.ToDto());
    }

    public async Task<Result<SettingsDto>> CreateAsync(CreateRestaurantSettingsDto dto)
    {
        var settings = new RestaurantSettings
        {
            RestaurantId = dto.RestaurantId,
            ReservationsNeedConfirmation = dto.ReservationsNeedConfirmation,
            MaxReservationDuration = dto.MaxReservationDuration,
            MinReservationDuration = dto.MinReservationDuration,
            MaxAdvanceBookingTime = dto.MaxAdvanceBookingTime,
            MinAdvanceBookingTime = dto.MinAdvanceBookingTime,
            MaxGuestsPerReservation = dto.MaxGuestsPerReservation,
            MinGuestsPerReservation = dto.MinGuestsPerReservation,
            ReservationsPerUserLimit = dto.ReservationsPerUserLimit
        };

        var created = await _repository.AddAsync(settings);
        return Result<SettingsDto>.Success(created.ToDto());
    }

    public async Task<Result<SettingsDto>> UpdateAsync(int id, UpdateRestaurantSettingsDto dto)
    {
        var existingSettings = await _repository.GetByIdAsync(id);

        existingSettings!.UpdateFromDto(dto);

        await _repository.UpdateAsync(existingSettings);
        return Result<SettingsDto>.Success(existingSettings.ToDto());
    }

    public async Task<Result> DeleteAsync(int id)
    {
        var settings = await _repository.GetByIdAsync(id);

        await _repository.DeleteAsync(settings!);
        return Result.Success();
    }

    public async Task<Result<bool>> ExistsAsync(int id)
    {
        var exists = await _repository.ExistsAsync(id);

        if (!exists)
            return Result<bool>.NotFound($"Restaurant settings with ID {id} not found.");

        return Result<bool>.Success(true);
    }

    public async Task<Result<bool>> NeedConfirmationAsync(int restaurantId)
    {
        var settings = await _repository.GetByRestaurantIdAsync(restaurantId);

        if (settings is null)
            return Result<bool>.NotFound($"Restaurant settings for restaurant {restaurantId} not found.");

        return Result<bool>.Success(settings.ReservationsNeedConfirmation);
    }
}