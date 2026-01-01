using RestaurantApp.Application.Interfaces.Repositories;
using RestaurantApp.Application.Interfaces.Services;
using RestaurantApp.Application.Mappers;
using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.DTOs.Menu.Variants;

namespace RestaurantApp.Application.Services;

public class MenuItemVariantService : IMenuItemVariantService
{
    private readonly IMenuItemVariantRepository _repository;

    public MenuItemVariantService(IMenuItemVariantRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<IEnumerable<MenuItemVariantDto>>> GetAllVariantsAsync(CancellationToken ct)
    {
        var variants = await _repository.GetAllAsync(ct);
        return Result<IEnumerable<MenuItemVariantDto>>.Success(variants.ToDtoList());
    }

    public async Task<Result<IEnumerable<MenuItemVariantDto>>> GetMenuItemVariantsAsync(int menuItemId, CancellationToken ct)
    {
        var variants = await _repository.GetByMenuItemIdAsync(menuItemId, ct);
        return Result<IEnumerable<MenuItemVariantDto>>.Success(variants.ToDtoList());
    }

    public async Task<Result<MenuItemVariantDto>> GetVariantByIdAsync(int id, CancellationToken ct)
    {
        var variant = await _repository.GetByIdAsync(id, ct);

        if (variant == null)
            return Result<MenuItemVariantDto>.NotFound("MenuItem variant not found.");

        return Result<MenuItemVariantDto>.Success(variant.ToDto());
    }

    public async Task<Result<MenuItemVariantDto>> CreateVariantAsync(MenuItemVariantDto variantDto, CancellationToken ct)
    {
        var newVariant = variantDto.ToEntity();
        var createdVariant = await _repository.AddAsync(newVariant, ct);

        return Result<MenuItemVariantDto>.Success(createdVariant.ToDto());
    }

    public async Task<Result<MenuItemVariantDto>> UpdateVariantAsync(int id, MenuItemVariantDto variantDto, CancellationToken ct)
    {
        var existingVariant = await _repository.GetByIdAsync(id, ct);

        existingVariant!.UpdateFromDto(variantDto);
        await _repository.UpdateAsync(existingVariant!, ct);

        return Result<MenuItemVariantDto>.Success(existingVariant!.ToDto());
    }

    public async Task<Result> DeleteVariantAsync(int id, CancellationToken ct)
    {
        var variant = await _repository.GetByIdAsync(id, ct);

        await _repository.DeleteAsync(variant!, ct);
        return Result.Success();
    }
}