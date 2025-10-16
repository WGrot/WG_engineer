using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.DTOs;
using RestaurantApp.Shared.Models;

namespace RestaurantApp.Api.Services.Interfaces;

public interface IMenuItemService
{
    Task<Result<MenuItemDto>> AddTagToMenuItemAsync(int menuItemId, int tagId);
    Task<Result<MenuItemDto>> RemoveTagFromMenuItemAsync(int menuItemId, int tagId);
    Task<Result<IEnumerable<MenuItemTagDto>>> GetMenuItemTagsAsync(int menuItemId);
}