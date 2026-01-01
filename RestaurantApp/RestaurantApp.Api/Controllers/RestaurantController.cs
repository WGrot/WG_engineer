using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantApp.Api.Common;
using RestaurantApp.Application.Interfaces.Services;
using RestaurantApp.Shared.DTOs.GeoCoding;
using RestaurantApp.Shared.DTOs.OpeningHours;
using RestaurantApp.Shared.DTOs.Restaurant;


namespace RestaurantApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RestaurantController : ControllerBase
{
    private readonly IRestaurantService _restaurantService;
    private readonly IRestaurantSearchService _restaurantSearchService;
    private readonly IRestaurantImageService _imageService;
    private readonly IRestaurantDashboardService _restaurantDashboardService;
    private readonly IRestaurantOpeningHoursService _restaurantOpeningHoursService;

    public RestaurantController(IRestaurantOpeningHoursService openingHoursService,
        IRestaurantSearchService restaurantSearchService, IRestaurantDashboardService restaurantDashboardService,
        IRestaurantService restaurantService,
        IRestaurantImageService imageService)
    {
        _restaurantService = restaurantService;
        _restaurantSearchService = restaurantSearchService;
        _restaurantDashboardService = restaurantDashboardService;
        _imageService = imageService;
        _restaurantOpeningHoursService = openingHoursService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var restaurantsResult = await _restaurantService.GetAllAsync();
        return restaurantsResult.ToActionResult();
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var restaurantResult = await _restaurantService.GetByIdAsync(id);

        return restaurantResult.ToActionResult();
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search(CancellationToken ct,
        [FromQuery] string? name = null,
        [FromQuery] string? address = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 5,
        [FromQuery] string sortBy = "newest"
        )
    {
        var restaurants = await _restaurantSearchService.SearchAsync(name, address, page, pageSize, sortBy);
        return restaurants.ToActionResult();
    }

    [HttpGet("open-now")]
    public async Task<IActionResult> GetOpenNow(CancellationToken ct)
    {
        var openRestaurantsResult = await _restaurantSearchService.GetOpenNowAsync();
        return openRestaurantsResult.ToActionResult();
    }

    [HttpGet("{id}/is-open")]
    public async Task<IActionResult> IsRestaurantOpen(
        int id,
        CancellationToken ct,
        [FromQuery] string? time = null,
        [FromQuery] int? dayOfWeek = null)
    {
        TimeOnly? checkTime = time != null ? TimeOnly.Parse(time) : null;
        DayOfWeek? checkDay = dayOfWeek != null ? (DayOfWeek)dayOfWeek : null;

        var statusResult = await _restaurantOpeningHoursService.CheckIfOpenAsync(id, ct, checkTime, checkDay);
        return statusResult.ToActionResult();
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create([FromBody] RestaurantDto restaurantDto, CancellationToken ct)
    {
        var createdRestaurantResult = await _restaurantService.CreateAsync(restaurantDto, ct);
        return createdRestaurantResult.ToActionResult();
    }

    [HttpPost("create-as-user")]
    [Authorize]
    public async Task<IActionResult> CreateAsUser([FromBody] CreateRestaurantDto restaurantDto, CancellationToken ct)
    {
        var createdRestaurantResult = await _restaurantService.CreateAsUserAsync(restaurantDto, ct);
        return createdRestaurantResult.ToActionResult();
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] RestaurantDto updateRestaurantDto, CancellationToken ct)
    {
        var result = await _restaurantService.UpdateAsync(id, updateRestaurantDto, ct);
        return result.ToActionResult();
    }

    [HttpPatch("{id}/basic-info")]
    public async Task<IActionResult> UpdateName(int id, [FromBody] RestaurantBasicInfoDto dto, CancellationToken ct)
    {
        var result = await _restaurantService.UpdateBasicInfoAsync(id, dto, ct);
        return result.ToActionResult();
    }

    [HttpPatch("{id}/opening-hours")]
    public async Task<IActionResult> UpdateOpeningHours(int id, [FromBody] List<OpeningHoursDto> openingHours, CancellationToken ct)
    {
        var result = await _restaurantOpeningHoursService.UpdateOpeningHoursAsync(id, openingHours, ct);
        return result.ToActionResult();
    }

    [HttpPatch("{id}/structured-address")]
    public async Task<IActionResult> UpdateStructuredAddress(int id, [FromBody] StructuresAddressDto newAddress, CancellationToken ct)
    {
        var result = await _restaurantService.UpdateStructuredAddressAsync(id, newAddress, ct);
        return result.ToActionResult();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var result = await _restaurantService.DeleteAsync(id, ct);
        return result.ToActionResult();
    }

    [HttpPost("{id}/upload-profile-photo")]
    public async Task<IActionResult> UploadRestaurantProfilePhoto(IFormFile image, int id, CancellationToken ct)
    {
        await using var stream = image.OpenReadStream();

        var result = await _imageService.UploadProfilePhotoAsync(
            id,
            stream,
            image.FileName, ct);

        return result.ToActionResult();
    }

    [HttpPost("{id}/upload-restaurant-photos")]
    public async Task<IActionResult> UploadRestaurantPhotos(List<IFormFile> imageList, int id, CancellationToken ct)
    {
        var images = imageList.Select(f => new ImageFileDto(f.OpenReadStream(), f.FileName));

        var result = await _imageService.UploadGalleryPhotosAsync(
            id,
            images, ct);
        return result.ToActionResult();
    }

    [HttpDelete("{id}/delete-profile-photo")]
    public async Task<IActionResult> DeleteProfilePhoto(int id, CancellationToken ct)
    {
        var result = await _imageService.DeleteProfilePhotoAsync(id, ct);
        return result.ToActionResult();
    }

    [HttpDelete("{id}/delete-photo")]
    public async Task<IActionResult> DeleteRestaurantPhoto(int id, int photoIndex, CancellationToken ct)
    {
        var result = await _imageService.DeleteGalleryPhotoAsync(id, photoIndex, ct);
        return result.ToActionResult();
    }

    [HttpGet("{id}/dashboard-data")]
    public async Task<IActionResult> GetDashboardData(int id, CancellationToken ct)
    {
        var result = await _restaurantDashboardService.GetDashboardDataAsync(id, ct);
        return result.ToActionResult();
    }

    [HttpGet("names")]
    public async Task<IActionResult> GetNames([FromQuery] List<int> ids, CancellationToken ct)
    {
        var result = await _restaurantService.GetRestaurantNamesAsync(ids, ct);
        return result.ToActionResult();
    }

    [HttpGet("nearby")]
    public async Task<IActionResult> GetNearbyRestaurants(
        [FromQuery] double latitude,
        [FromQuery] double longitude,
        CancellationToken ct,
        [FromQuery] double radius = 10
        )
    {
        var result = await _restaurantSearchService.GetNearbyRestaurantsAsync(
            latitude,
            longitude,
            ct,
            radius);

        return result.ToActionResult();
    }
}