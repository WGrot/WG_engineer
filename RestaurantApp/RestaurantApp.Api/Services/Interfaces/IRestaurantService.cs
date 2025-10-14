using RestaurantApp.Api.Common;
using RestaurantApp.Api.Models.DTOs;
using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.DTOs;
using RestaurantApp.Shared.Models;

namespace RestaurantApp.Api.Services.Interfaces;

public interface IRestaurantService
{
    Task<Result<IEnumerable<Restaurant>>> GetAllAsync();
    Task<Result<Restaurant>> GetByIdAsync(int id);
    Task<Result<IEnumerable<Restaurant>>> SearchAsync(string? name, string? address);
    Task<Result<IEnumerable<Table>>> GetTablesAsync(int restaurantId);
    Task<Result<IEnumerable<Restaurant>>> GetOpenNowAsync();
    Task<Result<OpenStatusDto>> CheckIfOpenAsync(int restaurantId, TimeOnly? time = null, DayOfWeek? dayOfWeek = null);
    Task<Result<Restaurant>> CreateAsync(RestaurantDto restaurantDto);
    Task<Result> UpdateAsync(int id, RestaurantDto restaurantDto);
    Task<Result> UpdateAddressAsync(int id, string address);
    Task<Result> UpdateNameAsync(int id, string name);
    Task<Result> UpdateOpeningHoursAsync(int id, List<OpeningHoursDto> openingHours);
    Task<Result> DeleteAsync(int id);
    Task<Result<ImageUploadResult>> UploadRestaurantProfilePhoto(IFormFile file, int restaurantId);
    Task<Result<List<ImageUploadResult>>> UploadRestaurantPhotos(List<IFormFile> imageList, int id);
    Task<Result> DeleteRestaurantProfilePicture(int restaurantId);
    Task<Result> DeleteRestaurantPhoto(int restaurantId, int photoIndex);
}