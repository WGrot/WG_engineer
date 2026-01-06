using Microsoft.EntityFrameworkCore;
using RestaurantApp.Application.Interfaces.Repositories;
using RestaurantApp.Domain.Models;

namespace RestaurantApp.Infrastructure.Persistence.Repositories;

public class MenuCategoryRepository : IMenuCategoryRepository
{
    private readonly ApplicationDbContext _context;

    public MenuCategoryRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<RepositoryResult<MenuCategory?>> GetByIdAsync(
        int categoryId, 
        CancellationToken ct, 
        bool includeItems = false)
    {
        try
        {
            var query = _context.MenuCategories.AsQueryable();

            if (includeItems)
            {
                query = query.Include(c => c.Items);
            }

            var result = await query.FirstOrDefaultAsync(c => c.Id == categoryId, ct);
            return RepositoryResult<MenuCategory?>.Success(result);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            return RepositoryResult<MenuCategory?>.Failure($"Failed to retrieve category with ID {categoryId}", ex);
        }
    }

    public async Task<RepositoryResult<IEnumerable<MenuCategory>>> GetActiveByMenuIdAsync(
        int? menuId, 
        CancellationToken ct)
    {
        try
        {
            var query = _context.MenuCategories
                .Include(c => c.Items)
                .Where(c => c.IsActive);

            if (menuId.HasValue)
            {
                query = query.Where(c => c.MenuId == menuId.Value);
            }

            var result = await query.OrderBy(c => c.DisplayOrder).ToListAsync(ct);
            return RepositoryResult<IEnumerable<MenuCategory>>.Success(result);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            return RepositoryResult<IEnumerable<MenuCategory>>.Failure(
                $"Failed to retrieve active categories for menu {menuId}", ex);
        }
    }

    public async Task<RepositoryResult<int>> GetMaxDisplayOrderAsync(int menuId, CancellationToken ct)
    {
        try
        {
            var result = await _context.MenuCategories
                .Where(c => c.MenuId == menuId)
                .MaxAsync(c => (int?)c.DisplayOrder, ct) ?? 0;
            
            return RepositoryResult<int>.Success(result);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            return RepositoryResult<int>.Failure($"Failed to get max display order for menu {menuId}", ex);
        }
    }

    public async Task<RepositoryResult<Menu?>> GetMenuByIdAsync(int menuId, CancellationToken ct)
    {
        try
        {
            var result = await _context.Menus.FindAsync(new object[] { menuId }, ct);
            return RepositoryResult<Menu?>.Success(result);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            return RepositoryResult<Menu?>.Failure($"Failed to retrieve menu with ID {menuId}", ex);
        }
    }

    public async Task<RepositoryResult> AddAsync(MenuCategory category, CancellationToken ct)
    {
        try
        {
            await _context.MenuCategories.AddAsync(category, ct);
            return RepositoryResult.Success();
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            return RepositoryResult.Failure("Failed to add category", ex);
        }
    }
    
    public RepositoryResult Remove(MenuCategory category, CancellationToken ct)
    {
        try
        {
            _context.MenuCategories.Remove(category);
            return RepositoryResult.Success();
        }
        catch (Exception ex)
        {
            return RepositoryResult.Failure($"Failed to remove category with ID {category.Id}", ex);
        }
    }

    public async Task<RepositoryResult> SaveChangesAsync(CancellationToken ct)
    {
        try
        {
            await _context.SaveChangesAsync(ct);
            return RepositoryResult.Success();
        }
        catch (DbUpdateConcurrencyException ex)
        {
            return RepositoryResult.Failure("Concurrency conflict occurred while saving changes", ex);
        }
        catch (DbUpdateException ex)
        {
            return RepositoryResult.Failure("Database error occurred while saving changes", ex);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            return RepositoryResult.Failure("Failed to save changes", ex);
        }
    }
}