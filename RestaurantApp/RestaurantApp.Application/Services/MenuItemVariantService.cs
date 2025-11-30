using RestaurantApp.Application.Interfaces.Repositories;
using RestaurantApp.Application.Interfaces.Services;
using RestaurantApp.Application.Mappers;
using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.DTOs.Menu.Variants;

namespace RestaurantApp.Application.Services;

public class MenuItemVariantService: IMenuItemVariantService
{
    private readonly IMenuItemVariantRepository _repository;

    public MenuItemVariantService(IMenuItemVariantRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<IEnumerable<MenuItemVariantDto>>> GetAllVariantsAsync()
    {
        var variants = await _repository.GetAllAsync();
        return Result<IEnumerable<MenuItemVariantDto>>.Success(variants.ToDtoList());
    }

    public async Task<Result<IEnumerable<MenuItemVariantDto>>> GetMenuItemVariantsAsync(int menuItemId)
    {
        var variants = await _repository.GetByMenuItemIdAsync(menuItemId);
        return Result<IEnumerable<MenuItemVariantDto>>.Success(variants.ToDtoList());
    }

    public async Task<Result<MenuItemVariantDto>> GetVariantByIdAsync(int id)
    {
        var variant = await _repository.GetByIdAsync(id);
        
        if (variant == null)
        {
            return Result<MenuItemVariantDto>.NotFound("MenuItem variant not found");
        }

        return Result<MenuItemVariantDto>.Success(variant.ToDto());
    }

    public async Task<Result<MenuItemVariantDto>> CreateVariantAsync(MenuItemVariantDto variantDto)
    {
        var menuItemExists = await _repository.MenuItemExistsAsync(variantDto.MenuItemId);
        
        if (!menuItemExists)
        {
            return Result<MenuItemVariantDto>.Failure($"MenuItem with id {variantDto.MenuItemId} does not exist");
        }

        var newVariant = variantDto.ToEntity();
        var createdVariant = await _repository.AddAsync(newVariant);

        return Result<MenuItemVariantDto>.Success(createdVariant.ToDto());
    }

    public async Task<Result<MenuItemVariantDto>> UpdateVariantAsync(int id, MenuItemVariantDto variantDto)
    {
        var existingVariant = await _repository.GetByIdAsync(id);
        
        if (existingVariant == null)
        {
            return Result<MenuItemVariantDto>.NotFound("MenuItem variant not found");
        }

        existingVariant.UpdateFromDto(variantDto);
        await _repository.UpdateAsync(existingVariant);

        return Result<MenuItemVariantDto>.Success(existingVariant.ToDto());
    }

    public async Task<Result> DeleteVariantAsync(int id)
    {
        var variant = await _repository.GetByIdAsync(id);
        
        if (variant == null)
        {
            return Result.Failure("MenuItemVariant not found");
        }

        await _repository.DeleteAsync(variant);
        return Result.Success();
    }
}