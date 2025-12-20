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
    public async Task<IActionResult> GetAll()
    {
        var result = await _restaurantSettingsService.GetAllAsync();
        return result.ToActionResult();
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _restaurantSettingsService.GetByIdAsync(id);
        return result.ToActionResult();
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateRestaurantSettingsDto restaurantSettingsDto)
    {
        var createdSettings = await _restaurantSettingsService.CreateAsync(restaurantSettingsDto);
        return createdSettings.ToActionResult();
    }

    [HttpGet("{restaurantId}/needs-confirmation")]
    public async Task<IActionResult> GetNeedsConfirmation(int restaurantId)
    {
        var result = await _restaurantSettingsService.NeedConfirmationAsync(restaurantId);
        return result.ToActionResult();
    }

    [HttpGet("{restaurantId}/get-restaurant-settings")]
    public async Task<IActionResult> GetByRestaurantId(int restaurantId)
    {
        var result = await _restaurantSettingsService.GetByRestaurantIdAsync(restaurantId);
        return result.ToActionResult();
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateRestaurantSettingsDto restaurantSettings)
    {
        var updatedSettings = await _restaurantSettingsService.UpdateAsync(id, restaurantSettings);
        return updatedSettings.ToActionResult();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _restaurantSettingsService.DeleteAsync(id);
        return deleted.ToActionResult();
    }

    [HttpHead("{id}")]
    public async Task<IActionResult> Exists(int id)
    {
        var exists = await _restaurantSettingsService.ExistsAsync(id);
        return exists.ToActionResult();
    }
}