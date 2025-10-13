using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantApp.Api.Common;
using RestaurantApp.Api.Models.DTOs;
using RestaurantApp.Api.Services.Interfaces;
using RestaurantApp.Shared.Models;

namespace RestaurantApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RestaurantController : ControllerBase
{
    private readonly IRestaurantService _restaurantService;
    private readonly ILogger<RestaurantController> _logger;

    public RestaurantController(IRestaurantService restaurantService, ILogger<RestaurantController> logger)
    {
        _restaurantService = restaurantService;
        _logger = logger;
    }

    // GET: api/Restaurant
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var restaurantsResult = await _restaurantService.GetAllAsync();
        return restaurantsResult.ToActionResult(this);
    }

    // GET: api/Restaurant/5
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var restaurantResult = await _restaurantService.GetByIdAsync(id);

        return restaurantResult.ToActionResult(this);
    }

    // GET: api/Restaurant/search?name=pizza
    [HttpGet("search")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Search(
        [FromQuery] string? name = null,
        [FromQuery] string? address = null)
    {
        var restaurants = await _restaurantService.SearchAsync(name, address);
        return restaurants.ToActionResult(this);
    }

    // GET: api/Restaurant/5/tables
    [HttpGet("{id}/tables")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetRestaurantTables(int id)
    {
        var tablesResult = await _restaurantService.GetTablesAsync(id);
        return tablesResult.ToActionResult(this);
    }

    // GET: api/Restaurant/open-now
    [HttpGet("open-now")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOpenNow()
    {
        var openRestaurantsResult = await _restaurantService.GetOpenNowAsync();
        return openRestaurantsResult.ToActionResult(this);
    }

    // GET: api/Restaurant/5/is-open
    [HttpGet("{id}/is-open")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> IsRestaurantOpen(
        int id,
        [FromQuery] string? time = null,
        [FromQuery] int? dayOfWeek = null)
    {
        TimeOnly? checkTime = time != null ? TimeOnly.Parse(time) : null;
        DayOfWeek? checkDay = dayOfWeek != null ? (DayOfWeek)dayOfWeek : null;

        var statusResult = await _restaurantService.CheckIfOpenAsync(id, checkTime, checkDay);
        return statusResult.ToActionResult(this);
    }

    // POST: api/Restaurant
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] RestaurantDto restaurantDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var createdRestaurantResult = await _restaurantService.CreateAsync(restaurantDto);
        return createdRestaurantResult.ToActionResult(this);
    }

    // PUT: api/Restaurant/5
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] RestaurantDto updateRestaurantDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _restaurantService.UpdateAsync(id, updateRestaurantDto);
        return result.ToActionResult(this);
    }

    // PATCH: api/Restaurant/5/address
    [HttpPatch("{restaurantId}/address")]
    [Authorize(Policy = "ManageMenu")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateAddress(int restaurantId, [FromBody] string address)
    {
            var result = await _restaurantService.UpdateAddressAsync(restaurantId, address);
            return result.ToActionResult(this);
    }

    // PATCH: api/Restaurant/5/address
    [HttpPatch("{id}/name")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateName(int id, [FromBody] string name)
    {
            var result = await _restaurantService.UpdateNameAsync(id, name);
            return result.ToActionResult(this);
    }

    // PATCH: api/Restaurant/5/opening-hours
    [HttpPatch("{id}/opening-hours")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateOpeningHours(int id, [FromBody] List<OpeningHoursDto> openingHours)
    {

            var result = await _restaurantService.UpdateOpeningHoursAsync(id, openingHours);
            return result.ToActionResult(this);

    }

    // DELETE: api/Restaurant/5
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(int id)
    {
            var result = await _restaurantService.DeleteAsync(id);
            return result.ToActionResult(this);
    }
    
    [HttpPost("{id}/upload-profile-photo")]
    public async Task <IActionResult> UploadRestaurantProfilePhoto(IFormFile image, int id)
    {
        var result = await _restaurantService.UploadRestaurantProfilePhoto(image, id);
        return result.ToActionResult(this);
    }
    
    [HttpPost("{id}/upload-restaurant-photos")]
    public async Task <IActionResult> UploadRestaurantPhotos(List<IFormFile> imageList, int id)
    {
        var result = await _restaurantService.UploadRestaurantPhotos(imageList, id);
        return result.ToActionResult(this);
    }
}