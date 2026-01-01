using Microsoft.AspNetCore.Mvc;
using RestaurantApp.Api.Common;
using RestaurantApp.Application.Interfaces.Services;
using RestaurantApp.Shared.DTOs.Settings;

namespace RestaurantApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RestaurantSettingsController : ControllerBase
{
    private readonly IRestaurantSettingsService _restaurantSettingsService;

    public RestaurantSettingsController(
        IRestaurantSettingsService restaurantSettingsService)
    {
        _restaurantSettingsService = restaurantSettingsService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await _restaurantSettingsService.GetAllAsync(ct);
        return result.ToActionResult();
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var result = await _restaurantSettingsService.GetByIdAsync(id, ct);
        return result.ToActionResult();
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateRestaurantSettingsDto restaurantSettingsDto, CancellationToken ct)
    {
        var createdSettings = await _restaurantSettingsService.CreateAsync(restaurantSettingsDto, ct);
        return createdSettings.ToActionResult();
    }

    [HttpGet("{restaurantId}/needs-confirmation")]
    public async Task<IActionResult> GetNeedsConfirmation(int restaurantId, CancellationToken ct)
    {
        var result = await _restaurantSettingsService.NeedConfirmationAsync(restaurantId, ct);
        return result.ToActionResult();
    }

    [HttpGet("{restaurantId}/get-restaurant-settings")]
    public async Task<IActionResult> GetByRestaurantId(int restaurantId, CancellationToken ct)
    {
        var result = await _restaurantSettingsService.GetByRestaurantIdAsync(restaurantId, ct);
        return result.ToActionResult();
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateRestaurantSettingsDto restaurantSettings, CancellationToken ct)
    {
        var updatedSettings = await _restaurantSettingsService.UpdateAsync(id, restaurantSettings, ct);
        return updatedSettings.ToActionResult();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var deleted = await _restaurantSettingsService.DeleteAsync(id, ct);
        return deleted.ToActionResult();
    }

    [HttpHead("{id}")]
    public async Task<IActionResult> Exists(int id, CancellationToken ct)
    {
        var exists = await _restaurantSettingsService.ExistsAsync(id, ct);
        return exists.ToActionResult();
    }
}