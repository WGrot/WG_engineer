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

    public async Task<Result<IEnumerable<SettingsDto>>> GetAllAsync(CancellationToken ct)
    {
        var settings = await _repository.GetAllAsync(ct);
        return Result<IEnumerable<SettingsDto>>.Success(settings.ToDtoList());
    }

    public async Task<Result<SettingsDto>> GetByIdAsync(int id, CancellationToken ct)
    {
        var settings = await _repository.GetByIdAsync(id, ct);

        if (settings is null)
            return Result<SettingsDto>.NotFound($"Restaurant settings with ID {id} not found.");

        return Result<SettingsDto>.Success(settings.ToDto());
    }

    public async Task<Result<SettingsDto>> GetByRestaurantIdAsync(int restaurantId, CancellationToken ct)
    {
        var settings = await _repository.GetByRestaurantIdAsync(restaurantId, ct);

        if (settings is null)
            return Result<SettingsDto>.NotFound($"Restaurant settings for restaurant {restaurantId} not found.");

        return Result<SettingsDto>.Success(settings.ToDto());
    }

    public async Task<Result<SettingsDto>> CreateAsync(CreateRestaurantSettingsDto dto, CancellationToken ct)
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

        var created = await _repository.AddAsync(settings, ct);
        return Result<SettingsDto>.Success(created.ToDto());
    }

    public async Task<Result<SettingsDto>> UpdateAsync(int id, UpdateRestaurantSettingsDto dto, CancellationToken ct)
    {
        var existingSettings = await _repository.GetByIdAsync(id, ct);

        existingSettings!.UpdateFromDto(dto);

        await _repository.UpdateAsync(existingSettings!, ct);
        return Result<SettingsDto>.Success(existingSettings!.ToDto());
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken ct)
    {
        var settings = await _repository.GetByIdAsync(id, ct);

        await _repository.DeleteAsync(settings!, ct);
        return Result.Success();
    }

    public async Task<Result<bool>> ExistsAsync(int id, CancellationToken ct)
    {
        var exists = await _repository.ExistsAsync(id, ct);

        if (!exists)
            return Result<bool>.NotFound($"Restaurant settings with ID {id} not found.");

        return Result<bool>.Success(true);
    }

    public async Task<Result<bool>> NeedConfirmationAsync(int restaurantId, CancellationToken ct)
    {
        var settings = await _repository.GetByRestaurantIdAsync(restaurantId, ct);

        if (settings is null)
            return Result<bool>.NotFound($"Restaurant settings for restaurant {restaurantId} not found.");

        return Result<bool>.Success(settings.ReservationsNeedConfirmation);
    }
}