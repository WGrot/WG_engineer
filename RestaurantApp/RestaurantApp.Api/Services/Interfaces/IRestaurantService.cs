using RestaurantApp.Api.Common;
using RestaurantApp.Domain.Models;
using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.DTOs;
using RestaurantApp.Shared.Models;

namespace RestaurantApp.Api.Services.Interfaces;

public interface IRestaurantService
{
    Task<Result<IEnumerable<Restaurant>>> GetAllAsync();
    Task<Result<Restaurant>> GetByIdAsync(int id);
    Task<Result<PaginatedRestaurantsDto>> SearchAsync(string? name, string? address, int page, int pageSize,
        string sortBy);
    Task<Result<IEnumerable<Table>>> GetTablesAsync(int restaurantId);
    Task<Result<IEnumerable<Restaurant>>> GetOpenNowAsync();
    Task<Result<OpenStatusDto>> CheckIfOpenAsync(int restaurantId, TimeOnly? time = null, DayOfWeek? dayOfWeek = null);
    Task<Result<Restaurant>> CreateAsync(RestaurantDto restaurantDto);
    Task<Result> UpdateAsync(int id, RestaurantDto restaurantDto);
    Task<Result> UpdateBasicInfoAsync(int id, RestaurantBasicInfoDto dto);
    Task<Result> UpdateOpeningHoursAsync(int id, List<OpeningHoursDto> openingHours);
    Task<Result> DeleteAsync(int id);
    Task<Result<ImageUploadResult>> UploadRestaurantProfilePhoto(IFormFile file, int restaurantId);
    Task<Result<List<ImageUploadResult>>> UploadRestaurantPhotos(List<IFormFile> imageList, int id);
    Task<Result> DeleteRestaurantProfilePicture(int restaurantId);
    Task<Result> DeleteRestaurantPhoto(int restaurantId, int photoIndex);
    
    Task<Result<RestaurantDashboardDataDto>> GetRestaurantDashboardData(int restaurantId);
}