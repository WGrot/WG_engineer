using Microsoft.EntityFrameworkCore;
using RestaurantApp.Api.Mappers;
using RestaurantApp.Api.Services.Interfaces;
using RestaurantApp.Domain.Models;
using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.DTOs;
using RestaurantApp.Shared.DTOs.Menu.Tags;
using RestaurantApp.Shared.Models;

namespace RestaurantApp.Api.Services;

public class MenuItemTagService : IMenuItemTagService
{
    private readonly ApiDbContext _context;
    private readonly ILogger<MenuItemTagService> _logger;

    public MenuItemTagService(ApiDbContext context, ILogger<MenuItemTagService> logger)
    {
        _context = context;
        _logger = logger;
    }
    
    public async Task<Result<IEnumerable<MenuItemTagDto>>> GetTagsAsync(int? restaurantId = null)
    {
        var query = _context.MenuItemTags.AsQueryable();

        if (restaurantId.HasValue)
        {
            query = query.Where(t => t.RestaurantId == restaurantId.Value);
        }

        var tags = await query.ToListAsync();
        return Result<IEnumerable<MenuItemTagDto>>.Success(tags.ToDto());
    }

    public async Task<Result<MenuItemTagDto?>> GetTagByIdAsync(int id)
    {
        var tag = await _context.MenuItemTags
            .FirstOrDefaultAsync(t => t.Id == id);
        if (tag == null)
        {
            return Result<MenuItemTagDto?>.NotFound("MenuItem tag not found");
        }

        return Result<MenuItemTag?>.Success(tag.ToDto());
    }

    public async Task<Result<MenuItemTagDto>> CreateTagAsync(CreateMenuItemTagDto tag)
    {
        var restaurantExists = await _context.Restaurants.AnyAsync(r => r.Id == tag.RestaurantId);
        if (!restaurantExists)
        {
            return Result<MenuItemTagDto>.Failure($"Restaurant with id {tag.RestaurantId} does not exist");
        }
        
        MenuItemTag newTag = tag.ToEntity();
        
        _context.MenuItemTags.Add(newTag);
        await _context.SaveChangesAsync();
        
        return Result<MenuItemTagDto>.Success(newTag.ToDto());
    }

    public async Task<Result<MenuItemTagDto>> UpdateTagAsync(int id, MenuItemTagDto tag)
    {
        var existingTag = await _context.MenuItemTags.FindAsync(id);
        if (existingTag == null)
        {
            return Result<MenuItemTagDto>.NotFound("MenuItem tag not found");
        }
        
        if (existingTag.RestaurantId != tag.RestaurantId)
        {
            var restaurantExists = await _context.Restaurants.AnyAsync(r => r.Id == tag.RestaurantId);
            if (!restaurantExists)
            {
                return Result<MenuItemTagDto>.Failure($"Restaurant with id {tag.RestaurantId} does not exist");
            }
        }
        
        existingTag.UpdateFromDto(tag);

        await _context.SaveChangesAsync();

        return Result<MenuItemTagDto>.Success(existingTag.ToDto());
    }

    public async Task<Result> DeleteTagAsync(int id)
    {

            var tag = await _context.MenuItemTags.FindAsync(id);
            if (tag == null)
            {
                return Result.Failure("MenuItem tag not found");
            }

            _context.MenuItemTags.Remove(tag);
            await _context.SaveChangesAsync();
            return Result.Success();
        }

    public async Task<Result<bool>> TagExistsAsync(int id)
    {
        bool result = await _context.MenuItemTags.AnyAsync(t => t.Id == id);
        return Result<bool>.Success(result);
    }


}