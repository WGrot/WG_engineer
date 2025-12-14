using Microsoft.EntityFrameworkCore;
using RestaurantApp.Application.Interfaces.Repositories;
using RestaurantApp.Domain.Enums;
using RestaurantApp.Domain.Models;

namespace RestaurantApp.Infrastructure.Persistence.Repositories;

public class RestaurantPermissionRepository: IRestaurantPermissionRepository
{
    private readonly ApplicationDbContext _context;

    public RestaurantPermissionRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<RestaurantPermission>> GetAllAsync()
    {
        return await _context.RestaurantPermissions
            .Include(p => p.RestaurantEmployee)
            .ToListAsync();
    }

    public async Task<RestaurantPermission?> GetByIdAsync(int id)
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

    public async Task<RestaurantPermission?> GetByEmployeeAndPermissionAsync(
        int employeeId, 
        PermissionType permission)
    {
        return await _context.RestaurantPermissions
            .Include(p => p.RestaurantEmployee)
            .FirstOrDefaultAsync(p => 
                p.RestaurantEmployeeId == employeeId && 
                p.Permission == permission);
    }

    public async Task<bool> ExistsAsync(int employeeId, PermissionType permission)
    {
        return await _context.RestaurantPermissions
            .AnyAsync(p => 
                p.RestaurantEmployeeId == employeeId && 
                p.Permission == permission);
    }

    public async Task<bool> ExistsExceptAsync(
        int employeeId, 
        PermissionType permission, 
        int excludeId)
    {
        return await _context.RestaurantPermissions
            .AnyAsync(p => 
                p.RestaurantEmployeeId == employeeId && 
                p.Permission == permission && 
                p.Id != excludeId);
    }

    public async Task<bool> EmployeeExistsAsync(int employeeId)
    {
        return await _context.RestaurantEmployees
            .AnyAsync(e => e.Id == employeeId);
    }

    public async Task<RestaurantPermission> AddAsync(RestaurantPermission permission)
    {
        _context.RestaurantPermissions.Add(permission);
        await _context.SaveChangesAsync();
        
        return await GetByIdAsync(permission.Id) ?? permission;
    }

    public async Task<RestaurantPermission> UpdateAsync(RestaurantPermission permission)
    {
        _context.RestaurantPermissions.Update(permission);
        await _context.SaveChangesAsync();
        
        return await GetByIdAsync(permission.Id) ?? permission;
    }

    public async Task DeleteAsync(RestaurantPermission permission)
    {
        _context.RestaurantPermissions.Remove(permission);
        await _context.SaveChangesAsync();
    }

    public async Task<int?> GetPermissionIdAsync(int employeeId, PermissionType permission)
    {
        return await _context.RestaurantPermissions
            .Where(p => 
                p.RestaurantEmployeeId == employeeId && 
                p.Permission == permission)
            .Select(p => (int?)p.Id)
            .FirstOrDefaultAsync();
    }

    public async Task AddRangeAsync(List<RestaurantPermission> permissions)
    {
        if (permissions.Count == 0)
            return;

        await _context.RestaurantPermissions.AddRangeAsync(permissions);
        await _context.SaveChangesAsync();
    }
}