using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantApp.Api.Common;
using RestaurantApp.Api.Services.Interfaces;
using RestaurantApp.Shared.DTOs;
using RestaurantApp.Shared.DTOs.GeoCoding;
using RestaurantApp.Shared.DTOs.OpeningHours;
using RestaurantApp.Shared.DTOs.Restaurant;
using RestaurantApp.Shared.Models;

namespace RestaurantApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RestaurantController : ControllerBase
{
    private readonly IRestaurantService _restaurantService;
    private readonly ILogger<RestaurantController> _logger;
    private readonly IRestaurantImageService _imageService;

    public RestaurantController(IRestaurantService restaurantService, ILogger<RestaurantController> logger, IRestaurantImageService imageService)
    {
        _restaurantService = restaurantService;
        _logger = logger;
        _imageService = imageService;
    }
    
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var restaurantsResult = await _restaurantService.GetAllAsync();
        return restaurantsResult.ToActionResult();
    }
    
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var restaurantResult = await _restaurantService.GetByIdAsync(id);

        return restaurantResult.ToActionResult();
    }
    
    [HttpGet("search")]
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
    
    [HttpGet("{id}/tables")]
    public async Task<IActionResult> GetRestaurantTables(int id)
    {
        var tablesResult = await _restaurantService.GetTablesAsync(id);
        return tablesResult.ToActionResult();
    }
    
    [HttpGet("open-now")]

    public async Task<IActionResult> GetOpenNow()
    {
        var openRestaurantsResult = await _restaurantService.GetOpenNowAsync();
        return openRestaurantsResult.ToActionResult();
    }
    
    [HttpGet("{id}/is-open")]
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
    
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create([FromBody] RestaurantDto restaurantDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var createdRestaurantResult = await _restaurantService.CreateAsync(restaurantDto);
        return createdRestaurantResult.ToActionResult();
    }
    
    [HttpPost("create-as-user")]
    [Authorize]
    public async Task<IActionResult> CreateAsUser([FromBody] CreateRestaurantDto restaurantDto)
    {
        var createdRestaurantResult = await _restaurantService.CreateAsUserAsync(restaurantDto);
        return createdRestaurantResult.ToActionResult();
    }
    
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] RestaurantDto updateRestaurantDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _restaurantService.UpdateAsync(id, updateRestaurantDto);
        return result.ToActionResult();
    }
    
    [HttpPatch("{id}/basic-info")]
    [Authorize(Policy = "RestaurantEmployee")]
    public async Task<IActionResult> UpdateName(int id, [FromBody] RestaurantBasicInfoDto dto)
    {
        var result = await _restaurantService.UpdateBasicInfoAsync(id, dto);
        return result.ToActionResult();
    }
    
    [HttpPatch("{id}/opening-hours")]
    public async Task<IActionResult> UpdateOpeningHours(int id, [FromBody] List<OpeningHoursDto> openingHours)
    {
        var result = await _restaurantService.UpdateOpeningHoursAsync(id, openingHours);
        return result.ToActionResult();
    }
    
    [HttpPatch("{id}/structured-address")]
    public async Task<IActionResult> UpdateStructuredAddress(int id, [FromBody] StructuresAddressDto newAddress)
    {
        var result = await _restaurantService.UpdateStructuredAddressAsync(id, newAddress);
        return result.ToActionResult();
    }
    
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _restaurantService.DeleteAsync(id);
        return result.ToActionResult();
    }

    [HttpPost("{id}/upload-profile-photo")]
    public async Task<IActionResult> UploadRestaurantProfilePhoto(IFormFile image, int id)
    {
        var result = await _imageService.UploadProfilePhotoAsync(id, image);
        return result.ToActionResult();
    }

    [HttpPost("{id}/upload-restaurant-photos")]
    public async Task<IActionResult> UploadRestaurantPhotos(List<IFormFile> imageList, int id)
    {
        var result = await _imageService.UploadGalleryPhotosAsync(id, imageList);
        return result.ToActionResult();
    }

    [HttpDelete("{id}/delete-profile-photo")]
    public async Task<IActionResult> DeleteProfilePhoto(int id)
    {
        var result = await _imageService.DeleteProfilePhotoAsync(id);
        return result.ToActionResult();
    }

    [HttpDelete("{id}/delete-photo")]
    public async Task<IActionResult> DeleteRestaurantPhoto(int id, int photoIndex)
    {
        var result = await _imageService.DeleteGalleryPhotoAsync(id, photoIndex);
        return result.ToActionResult();
    }
    
    [HttpGet("{id}/dashboard-data")]
    public async Task< IActionResult> GetDashboardData(int id){
        var result = await _restaurantService.GetRestaurantDashboardData(id);
        return result.ToActionResult();
    }
    
    [HttpGet("names")]
    public async Task<IActionResult> GetNames([FromQuery] List<int> ids)
    {
        var result = await _restaurantService.GetRestaurantNames(ids);
        return result.ToActionResult();
    }
    
    [HttpGet("nearby")]
    public async Task<IActionResult> GetNearbyRestaurants(
        [FromQuery] double latitude,
        [FromQuery] double longitude,
        [FromQuery] double radius = 10)
    {
        if (latitude < -90 || latitude > 90)
        {
            return BadRequest("Invalid latitude. Must be between -90 and 90.");
        }
    
        if (longitude < -180 || longitude > 180)
        {
            return BadRequest("Invalid longitude. Must be between -180 and 180.");
        }
    
        if (radius <= 0 || radius > 100)
        {
            radius = 100;
        }

        var result = await _restaurantService.GetNearbyRestaurantsAsync(
            latitude, 
            longitude, 
            radius);
        
        return result.ToActionResult();
    }
}