using Microsoft.EntityFrameworkCore;
using RestaurantApp.Api.Services.Interfaces;
using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.Common.Mappers;
using RestaurantApp.Shared.DTOs;
using RestaurantApp.Shared.Models;

namespace RestaurantApp.Api.Services;

public class MenuItemService : IMenuItemService
{
    private readonly ApiDbContext _context;
    private readonly ILogger<MenuItemTagService> _logger;

    public MenuItemService(ApiDbContext context, ILogger<MenuItemTagService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<MenuItemDto>> AddTagToMenuItemAsync(int menuItemId, int tagId)
    {
        var menuItem = await _context.MenuItems
            .Include(m => m.Tags)
            .FirstOrDefaultAsync(m => m.Id == menuItemId);

        if (menuItem == null)
        {
            return Result<MenuItemDto>.NotFound("MenuItem not found");
        }

        var tag = await _context.MenuItemTags.FindAsync(tagId);
        if (tag == null)
        {
            return Result<MenuItemDto>.NotFound("Tag not found");
        }


        if (menuItem.Tags.All(t => t.Id != tagId))
        {
            menuItem.Tags.Add(tag);
            await _context.SaveChangesAsync();
        }
        

        return Result<MenuItemDto>.Success(menuItem.ToDto());
    }

    public async Task<Result<MenuItemDto>> RemoveTagFromMenuItemAsync(int menuItemId, int tagId)
    {
        var menuItem = await _context.MenuItems
            .Include(m => m.Tags)
            .FirstOrDefaultAsync(m => m.Id == menuItemId);

        if (menuItem == null)
        {
            return Result<MenuItemDto>.NotFound("MenuItem not found");
        }

        var tag = menuItem.Tags.FirstOrDefault(t => t.Id == tagId);
        if (tag != null)
        {
            menuItem.Tags.Remove(tag);
            await _context.SaveChangesAsync();
        }
        

        return Result.Success(menuItem.ToDto());
    }

    public async Task<Result<IEnumerable<MenuItemTagDto>>> GetMenuItemTagsAsync(int menuItemId)
    {
        var menuItem = await _context.MenuItems
            .Include(m => m.Tags)
            .FirstOrDefaultAsync(m => m.Id == menuItemId);

        if (menuItem == null)
        {
            return Result<IEnumerable<MenuItemTagDto>>.NotFound("MenuItem not found");
        }

        var tagDtos = menuItem.Tags.Select(tag => tag.ToDto());
        return Result<IEnumerable<MenuItemTagDto>>.Success(tagDtos);
    }
}