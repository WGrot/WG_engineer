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

    public async Task<Result> ValidateTagExistsAsync(int tagId, CancellationToken ct)
    {
        var exists = await _tagRepository.ExistsAsync(tagId, ct);
        if (!exists)
            return Result.NotFound($"Menu item tag with ID {tagId} not found.");

        return Result.Success();
    }

    public async Task<Result> ValidateRestaurantExistsAsync(int restaurantId, CancellationToken ct)
    {
        var exists = await _restaurantRepository.ExistsAsync(restaurantId, ct);
        if (!exists)
            return Result.NotFound($"Restaurant with ID {restaurantId} not found.");

        return Result.Success();
    }

    public async Task<Result> ValidateForCreateAsync(CreateMenuItemTagDto dto, CancellationToken ct)
    {
        return await ValidateRestaurantExistsAsync(dto.RestaurantId, ct);
    }

    public async Task<Result> ValidateForUpdateAsync(int tagId, MenuItemTagDto dto, CancellationToken ct)
    {
        var tagResult = await ValidateTagExistsAsync(tagId, ct);
        if (!tagResult.IsSuccess)
            return tagResult;

        var tag = await _tagRepository.GetByIdAsync(tagId, ct);
        if (tag!.RestaurantId != dto.RestaurantId)
        {
            var restaurantResult = await ValidateRestaurantExistsAsync(dto.RestaurantId, ct);
            if (!restaurantResult.IsSuccess)
                return restaurantResult;
        }

        return Result.Success();
    }
}