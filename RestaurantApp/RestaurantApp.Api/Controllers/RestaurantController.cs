using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantApp.Api.Common;
using RestaurantApp.Api.Models.DTOs;
using RestaurantApp.Api.Services.Interfaces;
using RestaurantApp.Shared.DTOs;
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
        return restaurantsResult.ToActionResult();
    }

    // GET: api/Restaurant/5
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var restaurantResult = await _restaurantService.GetByIdAsync(id);

        return restaurantResult.ToActionResult();
    }

    // GET: api/Restaurant/search?name=pizza
    [HttpGet("search")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Search(
        [FromQuery] string? name = null,
        [FromQuery] string? address = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 5,
        [FromQuery] string sortBy = "newest")
    {
        var restaurants = await _restaurantService.SearchAsync(name, address, page, pageSize, sortBy);
        return restaurants.ToActionResult();
    }

    // GET: api/Restaurant/5/tables
    [HttpGet("{id}/tables")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetRestaurantTables(int id)
    {
        var tablesResult = await _restaurantService.GetTablesAsync(id);
        return tablesResult.ToActionResult();
    }

    // GET: api/Restaurant/open-now
    [HttpGet("open-now")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOpenNow()
    {
        var openRestaurantsResult = await _restaurantService.GetOpenNowAsync();
        return openRestaurantsResult.ToActionResult();
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
        return statusResult.ToActionResult();
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
        return createdRestaurantResult.ToActionResult();
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
        return result.ToActionResult();
    }
    

    // PATCH: api/Restaurant/5/address
    [HttpPatch("{id}/basic-info")]
    [Authorize(Policy = "RestaurantEmployee")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateName(int id, [FromBody] RestaurantBasicInfoDto dto)
    {
        var result = await _restaurantService.UpdateBasicInfoAsync(id, dto);
        return result.ToActionResult();
    }

    // PATCH: api/Restaurant/5/opening-hours
    [HttpPatch("{id}/opening-hours")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateOpeningHours(int id, [FromBody] List<OpeningHoursDto> openingHours)
    {
        var result = await _restaurantService.UpdateOpeningHoursAsync(id, openingHours);
        return result.ToActionResult();
    }

    // DELETE: api/Restaurant/5
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _restaurantService.DeleteAsync(id);
        return result.ToActionResult();
    }

    [HttpPost("{id}/upload-profile-photo")]
    public async Task<IActionResult> UploadRestaurantProfilePhoto(IFormFile image, int id)
    {
        var result = await _restaurantService.UploadRestaurantProfilePhoto(image, id);
        return result.ToActionResult();
    }

    [HttpPost("{id}/upload-restaurant-photos")]
    public async Task<IActionResult> UploadRestaurantPhotos(List<IFormFile> imageList, int id)
    {
        var result = await _restaurantService.UploadRestaurantPhotos(imageList, id);
        return result.ToActionResult();
    }

    [HttpDelete("{id}/delete-profile-photo")]
    public async Task<IActionResult> DeleteProfilePhoto(int id)
    {
        var result = await _restaurantService.DeleteRestaurantProfilePicture(id);
        return result.ToActionResult();
    }

    [HttpDelete("{id}/delete-photo")]
    public async Task<IActionResult> DeleteRestaurantPhoto(int id, int photoIndex)
    {
        var result = await _restaurantService.DeleteRestaurantPhoto(id, photoIndex);
        return result.ToActionResult();
    }
}