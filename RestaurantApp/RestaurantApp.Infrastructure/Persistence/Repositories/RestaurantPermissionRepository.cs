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

    public async Task<IEnumerable<RestaurantPermission>> GetAllAsync(CancellationToken ct)
    {
        return await _context.RestaurantPermissions
            .Include(p => p.RestaurantEmployee)
            .ToListAsync(cancellationToken: ct);
    }

    public async Task<RestaurantPermission?> GetByIdAsync(int id, CancellationToken ct)
    {
        return await _context.RestaurantPermissions
            .Include(p => p.RestaurantEmployee)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken: ct);
    }

    public async Task<IEnumerable<RestaurantPermission>> GetByEmployeeIdAsync(int employeeId, CancellationToken ct)
    {
        return await _context.RestaurantPermissions
            .Include(p => p.RestaurantEmployee)
            .Where(p => p.RestaurantEmployeeId == employeeId)
            .ToListAsync(cancellationToken: ct);
    }

    public async Task<IEnumerable<RestaurantPermission>> GetByRestaurantIdAsync(int restaurantId, CancellationToken ct)
    {
        return await _context.RestaurantPermissions
            .Include(p => p.RestaurantEmployee)
            .Where(p => p.RestaurantEmployee.RestaurantId == restaurantId)
            .ToListAsync(cancellationToken: ct);
    }

    public async Task<RestaurantPermission?> GetByEmployeeAndPermissionAsync(
        int employeeId, 
        PermissionType permission, CancellationToken ct)
    {
        return await _context.RestaurantPermissions
            .Include(p => p.RestaurantEmployee)
            .FirstOrDefaultAsync(p => 
                p.RestaurantEmployeeId == employeeId && 
                p.Permission == permission, cancellationToken: ct);
    }

    public async Task<bool> ExistsAsync(int employeeId, PermissionType permission, CancellationToken ct)
    {
        return await _context.RestaurantPermissions
            .AnyAsync(p => 
                p.RestaurantEmployeeId == employeeId && 
                p.Permission == permission, cancellationToken: ct);
    }

    public async Task<bool> ExistsExceptAsync(
        int employeeId, 
        PermissionType permission, 
        int excludeId, CancellationToken ct)
    {
        return await _context.RestaurantPermissions
            .AnyAsync(p => 
                p.RestaurantEmployeeId == employeeId && 
                p.Permission == permission && 
                p.Id != excludeId, cancellationToken: ct);
    }

    public async Task<bool> EmployeeExistsAsync(int employeeId, CancellationToken ct)
    {
        return await _context.RestaurantEmployees
            .AnyAsync(e => e.Id == employeeId, cancellationToken: ct);
    }

    public async Task<RestaurantPermission> AddAsync(RestaurantPermission permission, CancellationToken ct)
    {
        _context.RestaurantPermissions.Add(permission);
        await _context.SaveChangesAsync(ct);
        
        return await GetByIdAsync(permission.Id, ct) ?? permission;
    }

    public async Task<RestaurantPermission> UpdateAsync(RestaurantPermission permission, CancellationToken ct)
    {
        _context.RestaurantPermissions.Update(permission);
        await _context.SaveChangesAsync(ct);
        
        return await GetByIdAsync(permission.Id, ct) ?? permission;
    }

    public async Task DeleteAsync(RestaurantPermission permission, CancellationToken ct)
    {
        _context.RestaurantPermissions.Remove(permission);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<int?> GetPermissionIdAsync(int employeeId, PermissionType permission, CancellationToken ct)
    {
        return await _context.RestaurantPermissions
            .Where(p => 
                p.RestaurantEmployeeId == employeeId && 
                p.Permission == permission)
            .Select(p => (int?)p.Id)
            .FirstOrDefaultAsync(cancellationToken: ct);
    }

    public async Task AddRangeAsync(List<RestaurantPermission> permissions, CancellationToken ct)
    {
        if (permissions.Count == 0)
            return;

        await _context.RestaurantPermissions.AddRangeAsync(permissions, ct);
        await _context.SaveChangesAsync(ct);
    }
}