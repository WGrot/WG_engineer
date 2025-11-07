using Microsoft.EntityFrameworkCore;
using RestaurantApp.Api.Mappers;
using RestaurantApp.Api.Services.Interfaces;
using RestaurantApp.Domain.Models;
using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.DTOs;
using RestaurantApp.Shared.DTOs.Menu.Variants;
using RestaurantApp.Shared.Models;

namespace RestaurantApp.Api.Services;

public class MenuItemVariantService : IMenuItemVariantService
{
    private readonly ApiDbContext _context;


    public MenuItemVariantService(ApiDbContext context)
    {
        _context = context;

    }

    public async Task<Result<IEnumerable<MenuItemVariantDto>>> GetAllVariantsAsync()
    {
        var variants = await _context.MenuItemVariants
            .ToListAsync();

        return Result<IEnumerable<MenuItemVariantDto>>.Success(variants.ToDtoList());
    }

    public async Task<Result<IEnumerable<MenuItemVariantDto>>> GetMenuItemVariantsAsync(int id)
    {
        var result = await _context.MenuItemVariants
            .Where(v => v.MenuItemId == id)
            .ToListAsync();
        
        return Result<IEnumerable<MenuItemVariantDto>>.Success(result.ToDtoList());
    }

    public async Task<Result<MenuItemVariantDto?>> GetVariantByIdAsync(int id)
    {
        var variant = await _context.MenuItemVariants
            .FirstOrDefaultAsync(t => t.Id == id);
        if (variant == null)
        {
            return Result<MenuItemVariantDto?>.NotFound("MenuItem variant not found");
        }

        return Result<MenuItemVariantDto?>.Success(variant.ToDto());
    }

    public async Task<Result<MenuItemVariantDto>> CreateVariantAsync(MenuItemVariantDto variant)
    {
        var menuItem = await _context.MenuItems.AnyAsync(i => i.Id == variant.MenuItemId);
        if (!menuItem)
        {
            return Result<MenuItemVariantDto>.Failure($"Restaurant with id {variant.Id} does not exist");
        }
        
        MenuItemVariant newVariant = variant.ToEntity();
        
        _context.MenuItemVariants.Add(newVariant);
        await _context.SaveChangesAsync();
        
        return Result<MenuItemVariantDto>.Success(newVariant.ToDto());
    }

    public async Task<Result<MenuItemVariantDto?>> UpdateVariantAsync(int id, MenuItemVariantDto variantDto)
    {
        var existingVariant = await _context.MenuItemVariants.FindAsync(id);
        if (existingVariant == null)
        {
            return Result<MenuItemVariantDto?>.NotFound("MenuItem variantDto not found");
        }
        
        
        existingVariant.UpdateFromDto(variantDto);

        await _context.SaveChangesAsync();

        return Result<MenuItemVariantDto?>.Success(existingVariant.ToDto());
    }

    public async Task<Result> DeleteVariantAsync(int id)
    {
        var variant = await _context.MenuItemVariants.FindAsync(id);
        if (variant == null)
        {
            return Result.Failure("MenuItemVariant not found");
        }

        _context.MenuItemVariants.Remove(variant);
        await _context.SaveChangesAsync();
        return Result.Success();
    }
}