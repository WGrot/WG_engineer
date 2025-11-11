using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.DTOs;
using RestaurantApp.Shared.DTOs.Menu.Tags;
using RestaurantApp.Shared.Models;

namespace RestaurantApp.Api.Services.Interfaces;

public interface IMenuItemTagService
{
    Task<Result<IEnumerable<MenuItemTagDto>>> GetTagsAsync(int? restaurantId = null);
    Task<Result<MenuItemTagDto?>> GetTagByIdAsync(int id);
    Task<Result<MenuItemTagDto>> CreateTagAsync(CreateMenuItemTagDto tag);
    Task<Result<MenuItemTagDto?>> UpdateTagAsync(int id, MenuItemTagDto tag);
    Task<Result> DeleteTagAsync(int id);
    Task<Result<bool>> TagExistsAsync(int id);
}