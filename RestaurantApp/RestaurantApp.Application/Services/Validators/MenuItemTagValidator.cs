using RestaurantApp.Application.Interfaces.Repositories;
using RestaurantApp.Application.Interfaces.Validators;
using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.DTOs.Menu.Tags;

namespace RestaurantApp.Application.Services.Validators;

public class MenuItemTagValidator: IMenuItemTagValidator
{
    private readonly IMenuItemTagRepository _tagRepository;
    private readonly IRestaurantRepository _restaurantRepository;

    public MenuItemTagValidator(
        IMenuItemTagRepository tagRepository,
        IRestaurantRepository restaurantRepository)
    {
        _tagRepository = tagRepository;
        _restaurantRepository = restaurantRepository;
    }

    public async Task<Result> ValidateTagExistsAsync(int tagId)
    {
        var exists = await _tagRepository.ExistsAsync(tagId);
        if (!exists)
            return Result.NotFound($"Menu item tag with ID {tagId} not found.");

        return Result.Success();
    }

    public async Task<Result> ValidateRestaurantExistsAsync(int restaurantId)
    {
        var exists = await _restaurantRepository.ExistsAsync(restaurantId);
        if (!exists)
            return Result.NotFound($"Restaurant with ID {restaurantId} not found.");

        return Result.Success();
    }

    public async Task<Result> ValidateForCreateAsync(CreateMenuItemTagDto dto)
    {
        return await ValidateRestaurantExistsAsync(dto.RestaurantId);
    }

    public async Task<Result> ValidateForUpdateAsync(int tagId, MenuItemTagDto dto)
    {
        var tagResult = await ValidateTagExistsAsync(tagId);
        if (!tagResult.IsSuccess)
            return tagResult;

        var tag = await _tagRepository.GetByIdAsync(tagId);
        if (tag!.RestaurantId != dto.RestaurantId)
        {
            var restaurantResult = await ValidateRestaurantExistsAsync(dto.RestaurantId);
            if (!restaurantResult.IsSuccess)
                return restaurantResult;
        }

        return Result.Success();
    }
}