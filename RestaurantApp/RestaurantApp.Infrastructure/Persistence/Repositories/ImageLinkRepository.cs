using Microsoft.EntityFrameworkCore;
using RestaurantApp.Application.Interfaces.Repositories;
using RestaurantApp.Domain.Enums;
using RestaurantApp.Domain.Models;

namespace RestaurantApp.Infrastructure.Persistence.Repositories;

public class ImageLinkRepository : IImageLinkRepository
{
    private readonly ApplicationDbContext _context;
    
    public ImageLinkRepository(ApplicationDbContext context)
    {
        _context = context;
    }
    
    public async Task<ImageLink?> GetByIdAsync(int id)
    {
        return await _context.ImageLinks.FindAsync(id);
    }

    public async Task<IEnumerable<ImageLink>> GetByRestaurantAndTypeAsync(int restaurantId, ImageType type)
    {
        return await _context.ImageLinks
            .Where(i => i.RestaurantId == restaurantId && i.Type == type)
            .OrderBy(i => i.Type)
            .ThenBy(i => i.SortOrder)
            .ToListAsync();
    }

    public async Task<IEnumerable<ImageLink>> GetByRestaurantIdAsync(int restaurantId)
    {
        return await _context.ImageLinks
            .Where(i => i.RestaurantId == restaurantId)
            .OrderBy(i => i.Type)
            .ThenBy(i => i.SortOrder)
            .ToListAsync();
    }
    

    public async Task<int> GetMaxSortOrderAsync(int restaurantId, ImageType type)
    {
        var maxOrder = await _context.ImageLinks
            .Where(i => i.RestaurantId == restaurantId && i.Type == type)
            .MaxAsync(i => (int?)i.SortOrder);
        
        return maxOrder ?? 0;
    }

    public async Task AddAsync(ImageLink image)
    {
        await _context.ImageLinks.AddAsync(image);
    }

    public async Task Remove(ImageLink image)
    {
         _context.ImageLinks.Remove(image);
         await _context.SaveChangesAsync();
    }

    public async Task SaveChangesAsync()
    {
       await _context.SaveChangesAsync();
    }
}