using Microsoft.EntityFrameworkCore;
using RestaurantApp.Application.Interfaces.Repositories;
using RestaurantApp.Application.Mappers.EnumMappers;
using RestaurantApp.Domain.Models;
using RestaurantApp.Shared.Models;

namespace RestaurantApp.Infrastructure.Persistence.Repositories;

public class RestaurantEmployeeRepository : IRestaurantEmployeeRepository
{
    private readonly ApplicationDbContext _context;

    public RestaurantEmployeeRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<RestaurantEmployee>> GetAllWithDetailsAsync()
    {
        return await _context.RestaurantEmployees
            .Include(e => e.Restaurant)
            .Include(e => e.Permissions)
            .ToListAsync();
    }

    public async Task<RestaurantEmployee?> GetByIdWithDetailsAsync(int id)
    {
        return await _context.RestaurantEmployees
            .Include(e => e.Restaurant)
            .Include(e => e.Permissions)
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<IEnumerable<RestaurantEmployee>> GetByRestaurantIdWithDetailsAsync(int restaurantId)
    {
        return await _context.RestaurantEmployees
            .Include(e => e.Restaurant)
            .Include(e => e.Permissions)
            .Where(e => e.RestaurantId == restaurantId)
            .ToListAsync();
    }

    public async Task<IEnumerable<RestaurantEmployee>> GetByUserIdWithDetailsAsync(string userId)
    {
        return await _context.RestaurantEmployees
            .Include(e => e.Restaurant)
            .Include(e => e.Permissions)
            .Where(e => e.UserId == userId)
            .ToListAsync();
    }

    public async Task<RestaurantEmployee?> GetByIdAsync(int id)
    {
        return await _context.RestaurantEmployees.FindAsync(id);
    }

    public async Task AddAsync(RestaurantEmployee employee)
    {
        await _context.RestaurantEmployees.AddAsync(employee);
    }

    public void Update(RestaurantEmployee employee)
    {
        _context.RestaurantEmployees.Update(employee);
    }

    public void Remove(RestaurantEmployee employee)
    {
        _context.RestaurantEmployees.Remove(employee);
    }

    public async Task AddPermissionsAsync(int employeeId, IEnumerable<PermissionTypeEnumDto> permissions)
    {
        var permissionEntities = permissions.Select(p => new RestaurantPermission
        {
            RestaurantEmployeeId = employeeId,
            Permission = p.ToDomain()
        });

        await _context.RestaurantPermissions.AddRangeAsync(permissionEntities);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}