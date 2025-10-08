using Microsoft.AspNetCore.Mvc;
using RestaurantApp.Api.Common;
using RestaurantApp.Api.Models.DTOs;
using RestaurantApp.Api.Services.Interfaces;
using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.Models;

namespace RestaurantApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RestaurantSettingsController : ControllerBase
{
    private readonly IRestaurantSettingsService _restaurantSettingsService;
    private readonly IRestaurantService _restaurantService;
    private readonly ILogger<RestaurantSettingsController> _logger;

    public RestaurantSettingsController(
        IRestaurantSettingsService restaurantSettingsService,
        IRestaurantService restaurantService,
        ILogger<RestaurantSettingsController> logger)
    {
        _restaurantSettingsService = restaurantSettingsService;
        _restaurantService = restaurantService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _restaurantSettingsService.GetAllAsync();
        return result.ToActionResult(this);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _restaurantSettingsService.GetByIdAsync(id);
        return result.ToActionResult(this);
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateRestaurantSettingsDto restaurantSettingsDto)
    {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var restaurant = await _restaurantService.GetByIdAsync(restaurantSettingsDto.RestaurantId);

            if (restaurant.IsFailure)
            {
                return restaurant.ToActionResult(this);
            }
            
            RestaurantSettings restaurantSettings = new RestaurantSettings
            {
                RestaurantId = restaurantSettingsDto.RestaurantId,
                ReservationsNeedConfirmation = restaurantSettingsDto.ReservationsNeedConfirmation,
                Restaurant = restaurant.Value
            };

            var createdSettings = await _restaurantSettingsService.CreateAsync(restaurantSettings);
            return createdSettings.ToActionResult(this);

    }

    [HttpGet("{restaurantId}/needs-confirmation")]
    public async Task<IActionResult> GetNeedsConfirmation(int restaurantId)
    {
        var result = await _restaurantSettingsService.NeedConfirmation(restaurantId);
        return result.ToActionResult(this);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] RestaurantSettings restaurantSettings)
    {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var updatedSettings = await _restaurantSettingsService.UpdateAsync(id, restaurantSettings);

            return updatedSettings.ToActionResult(this);

    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {

            var deleted = await _restaurantSettingsService.DeleteAsync(id);

            return deleted.ToActionResult(this);
    }

    [HttpHead("{id}")]
    public async Task<IActionResult> Exists(int id)
    {

            var exists = await _restaurantSettingsService.ExistsAsync(id);


            return exists.ToActionResult(this);


    }
}