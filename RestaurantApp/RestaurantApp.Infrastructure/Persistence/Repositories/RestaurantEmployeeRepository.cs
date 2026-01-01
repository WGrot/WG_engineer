using Microsoft.EntityFrameworkCore;
using RestaurantApp.Application.Interfaces.Repositories;
using RestaurantApp.Application.Mappers.EnumMappers;
using RestaurantApp.Domain.Models;
using RestaurantApp.Shared.DTOs.Auth;
using RestaurantApp.Shared.Models;

namespace RestaurantApp.Infrastructure.Persistence.Repositories;

public class RestaurantEmployeeRepository : IRestaurantEmployeeRepository
{
    private readonly ApplicationDbContext _context;

    public RestaurantEmployeeRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<RestaurantEmployee>> GetAllWithDetailsAsync(CancellationToken ct)
    {
        return await _context.RestaurantEmployees
            .Include(e => e.Restaurant)
            .Include(e => e.Permissions)
            .ToListAsync(cancellationToken: ct);
    }

    public async Task<RestaurantEmployee?> GetByIdWithDetailsAsync(int id, CancellationToken ct)
    {
        return await _context.RestaurantEmployees
            .Include(e => e.Restaurant)
            .Include(e => e.Permissions)
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken: ct);
    }

    public async Task<IEnumerable<RestaurantEmployee>> GetByRestaurantIdWithDetailsAsync(int restaurantId, CancellationToken ct)
    {
        return await _context.RestaurantEmployees
            .Include(e => e.Restaurant)
            .Include(e => e.Permissions)
            .Where(e => e.RestaurantId == restaurantId)
            .ToListAsync(cancellationToken: ct);
    }

    public async Task<IEnumerable<RestaurantEmployee>> GetByUserIdWithDetailsAsync(string userId, CancellationToken ct)
    {
        return await _context.RestaurantEmployees
            .Include(e => e.Restaurant)
            .Include(e => e.Permissions)
            .Where(e => e.UserId == userId)
            .ToListAsync(cancellationToken: ct);
    }

    public async Task<RestaurantEmployee?> GetByIdAsync(int id, CancellationToken ct)
    {
        return await _context.RestaurantEmployees.FindAsync(id, ct);
    }

    public async Task AddAsync(RestaurantEmployee employee, CancellationToken ct)
    {
        await _context.RestaurantEmployees.AddAsync(employee, ct);
    }

    public void Update(RestaurantEmployee employee, CancellationToken ct)
    {
        _context.RestaurantEmployees.Update(employee);
    }

    public void Remove(RestaurantEmployee employee, CancellationToken ct)
    {
        _context.RestaurantEmployees.Remove(employee);
    }

    public async Task AddPermissionsAsync(int employeeId, IEnumerable<PermissionTypeEnumDto> permissions, CancellationToken ct)
    {
        var permissionEntities = permissions.Select(p => new RestaurantPermission
        {
            RestaurantEmployeeId = employeeId,
            Permission = p.ToDomain()
        });

        await _context.RestaurantPermissions.AddRangeAsync(permissionEntities, ct);
    }
    
    public async Task<List<EmployeeClaimsDto>> GetEmployeeClaimsDataAsync(string userId, CancellationToken ct)
    {
        return await _context.RestaurantEmployees
            .Where(e => e.UserId == userId)
            .Include(e => e.Permissions)
            .Select(e => new EmployeeClaimsDto
            {
                Id = e.Id,
                RestaurantId = e.RestaurantId,
                Role = e.Role.ToString(),
                Permissions = e.Permissions
                    .Select(p => p.Permission.ToString())
                    .ToList()
            })
            .ToListAsync(cancellationToken: ct);
    }

    public async Task SaveChangesAsync(CancellationToken ct)
    {
        await _context.SaveChangesAsync(ct);
    }
}