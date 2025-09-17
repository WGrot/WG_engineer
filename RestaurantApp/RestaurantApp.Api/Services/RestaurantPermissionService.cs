using Microsoft.EntityFrameworkCore;
using RestaurantApp.Api.Services.Interfaces;
using RestaurantApp.Shared.Models;

namespace RestaurantApp.Api.Services;

public class RestaurantPermissionService : IRestaurantPermissionService
{
    private readonly ApiDbContext _context;
    
    public RestaurantPermissionService(ApiDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<RestaurantPermission>> GetAllAsync()
    {
        return await _context.RestaurantPermissions
            .Include(p => p.RestaurantEmployee)
            .ToListAsync();
    }

    public async Task<RestaurantPermission> GetByIdAsync(int id)
    {
        return await _context.RestaurantPermissions
            .Include(p => p.RestaurantEmployee)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<IEnumerable<RestaurantPermission>> GetByEmployeeIdAsync(int employeeId)
    {
        return await _context.RestaurantPermissions
            .Include(p => p.RestaurantEmployee)
            .Where(p => p.RestaurantEmployeeId == employeeId)
            .ToListAsync();
    }

    public async Task<IEnumerable<RestaurantPermission>> GetByRestaurantIdAsync(int restaurantId)
    {
        return await _context.RestaurantPermissions
            .Include(p => p.RestaurantEmployee)
            .Where(p => p.RestaurantEmployee.RestaurantId == restaurantId)
            .ToListAsync();
    }

    public async Task<RestaurantPermission> CreateAsync(RestaurantPermission permission)
    {
        _context.RestaurantPermissions.Add(permission);
        await _context.SaveChangesAsync();
        
        return await GetByIdAsync(permission.Id);
    }

    public async Task<RestaurantPermission> UpdateAsync(RestaurantPermission permission)
    {
        _context.Entry(permission).State = EntityState.Modified;
        
        try
        {
            await _context.SaveChangesAsync();
            return await GetByIdAsync(permission.Id);
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await PermissionExistsAsync(permission.Id))
                return null;
            throw;
        }
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var permission = await _context.RestaurantPermissions.FindAsync(id);
        if (permission == null)
            return false;

        _context.RestaurantPermissions.Remove(permission);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> HasPermissionAsync(int employeeId, PermissionType permission)
    {
        return await _context.RestaurantPermissions
            .AnyAsync(p => p.RestaurantEmployeeId == employeeId && p.Permission == permission);
    }

    private async Task<bool> PermissionExistsAsync(int id)
    {
        return await _context.RestaurantPermissions.AnyAsync(p => p.Id == id);
    }
}