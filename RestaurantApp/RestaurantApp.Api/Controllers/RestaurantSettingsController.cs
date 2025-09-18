using Microsoft.AspNetCore.Mvc;
using RestaurantApp.Api.Models.DTOs;
using RestaurantApp.Api.Services.Interfaces;
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
    public async Task<ActionResult<IEnumerable<RestaurantSettings>>> GetAll()
    {
        try
        {
            var settings = await _restaurantSettingsService.GetAllAsync();
            return Ok(settings);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting all restaurant settings");
            return StatusCode(500, "An error occurred while processing your request");
        }
    }
    
    [HttpGet("{id}")]
    public async Task<ActionResult<RestaurantSettings>> GetById(int id)
    {
        try
        {
            var settings = await _restaurantSettingsService.GetByIdAsync(id);
            if (settings == null)
            {
                return NotFound($"Restaurant settings with ID {id} not found");
            }

            return Ok(settings);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting restaurant settings with id {Id}", id);
            return StatusCode(500, "An error occurred while processing your request");
        }
    }
    
    [HttpPost]
    public async Task<ActionResult<RestaurantSettings>> Create([FromBody] CreateRestaurantSettingsDto restaurantSettingsDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            Restaurant restaurant = await _restaurantService.GetByIdAsync(restaurantSettingsDto.RestaurantId);
            
            RestaurantSettings restaurantSettings = new RestaurantSettings
            {
                RestaurantId = restaurantSettingsDto.RestaurantId,
                ReservationsNeedConfirmation = restaurantSettingsDto.ReservationsNeedConfirmation,
                Restaurant = restaurant
            };

            var createdSettings = await _restaurantSettingsService.CreateAsync(restaurantSettings);
            return CreatedAtAction(nameof(GetById), new { id = createdSettings.Id }, createdSettings);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating restaurant settings");
            return StatusCode(500, "An error occurred while processing your request");
        }
    }
    
    [HttpPut("{id}")]
    public async Task<ActionResult<RestaurantSettings>> Update(int id, [FromBody] RestaurantSettings restaurantSettings)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var updatedSettings = await _restaurantSettingsService.UpdateAsync(id, restaurantSettings);
            if (updatedSettings == null)
            {
                return NotFound($"Restaurant settings with ID {id} not found");
            }

            return Ok(updatedSettings);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating restaurant settings with id {Id}", id);
            return StatusCode(500, "An error occurred while processing your request");
        }
    }
    
    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        try
        {
            var deleted = await _restaurantSettingsService.DeleteAsync(id);
            if (!deleted)
            {
                return NotFound($"Restaurant settings with ID {id} not found");
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting restaurant settings with id {Id}", id);
            return StatusCode(500, "An error occurred while processing your request");
        }
    }
    
    [HttpHead("{id}")]
    public async Task<ActionResult> Exists(int id)
    {
        try
        {
            var exists = await _restaurantSettingsService.ExistsAsync(id);
            if (!exists)
            {
                return NotFound();
            }

            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while checking if restaurant settings exists with id {Id}", id);
            return StatusCode(500);
        }
    }
}