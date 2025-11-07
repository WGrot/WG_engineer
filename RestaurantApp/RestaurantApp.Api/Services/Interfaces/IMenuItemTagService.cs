using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.DTOs;
using RestaurantApp.Shared.DTOs.Menu.Tags;
using RestaurantApp.Shared.Models;

namespace RestaurantApp.Api.Services.Interfaces;

public interface IMenuItemTagService
{
    Task<Result<IEnumerable<MenuItemTagDto>>> GetAllTagsAsync();
    Task<Result<IEnumerable<MenuItemTagDto>>> GetTagsByRestaurantIdAsync(int restaurantId);
    Task<Result<MenuItemTagDto?>> GetTagByIdAsync(int id);
    Task<Result<MenuItemTagDto>> CreateTagAsync(MenuItemTagDto tag);
    Task<Result<MenuItemTagDto?>> UpdateTagAsync(int id, MenuItemTagDto tag);
    Task<Result> DeleteTagAsync(int id);
    Task<Result<bool>> TagExistsAsync(int id);
}