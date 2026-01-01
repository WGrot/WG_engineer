using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.DTOs.Menu.Tags;

namespace RestaurantApp.Application.Interfaces.Services;

public interface IMenuItemTagService
{
    Task<Result<IEnumerable<MenuItemTagDto>>> GetTagsAsync(CancellationToken ct = default, int? restaurantId = null);
    Task<Result<MenuItemTagDto?>> GetTagByIdAsync(int id, CancellationToken ct = default);
    Task<Result<MenuItemTagDto>> CreateTagAsync(CreateMenuItemTagDto tag, CancellationToken ct = default);
    Task<Result<MenuItemTagDto>> UpdateTagAsync(int id, MenuItemTagDto tag, CancellationToken ct = default);
    Task<Result> DeleteTagAsync(int id, CancellationToken ct = default);
    Task<Result<bool>> TagExistsAsync(int id, CancellationToken ct = default);
}