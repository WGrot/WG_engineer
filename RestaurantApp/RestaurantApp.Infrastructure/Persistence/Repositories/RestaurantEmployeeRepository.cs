using RestaurantApp.Application.Interfaces.Repositories;
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

    public async Task AddAsync(RestaurantEmployee employee)
    {
        await _context.RestaurantEmployees.AddAsync(employee);
    }

    public async Task AddPermissionsAsync(int employeeId, IEnumerable<PermissionType> permissions)
    {
        var permissionEntities = permissions.Select(p => new RestaurantPermission
        {
            RestaurantEmployeeId = employeeId,
            Permission = p
        });

        await _context.RestaurantPermissions.AddRangeAsync(permissionEntities);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}