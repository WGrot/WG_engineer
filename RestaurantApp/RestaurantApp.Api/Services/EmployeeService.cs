using Microsoft.EntityFrameworkCore;
using RestaurantApp.Api.Services.Interfaces;
using RestaurantApp.Shared.Models;

namespace RestaurantApp.Api.Services;

public class EmployeeService : IEmployeeService
{
    private readonly ApiDbContext _context;
    
    public EmployeeService(ApiDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<RestaurantEmployee>> GetAllAsync()
    {
        return await _context.RestaurantEmployees
            .Include(e => e.Restaurant)
            .Include(e => e.Permissions)
            .ToListAsync();
    }

    public async Task<RestaurantEmployee> GetByIdAsync(int id)
    {
        return await _context.RestaurantEmployees
            .Include(e => e.Restaurant)
            .Include(e => e.Permissions)
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<IEnumerable<RestaurantEmployee>> GetByRestaurantIdAsync(int restaurantId)
    {
        return await _context.RestaurantEmployees
            .Include(e => e.Restaurant)
            .Include(e => e.Permissions)
            .Where(e => e.RestaurantId == restaurantId)
            .ToListAsync();
    }

    public async Task<IEnumerable<RestaurantEmployee>> GetByUserIdAsync(string userId)
    {
        return await _context.RestaurantEmployees
            .Include(e => e.Restaurant)
            .Include(e => e.Permissions)
            .Where(e => e.UserId == userId)
            .ToListAsync();
    }

    public async Task<RestaurantEmployee> CreateAsync(RestaurantEmployee employee)
    {
        employee.CreatedAt = DateTime.UtcNow;
        employee.IsActive = true;
        
        _context.RestaurantEmployees.Add(employee);
        await _context.SaveChangesAsync();
        
        return await GetByIdAsync(employee.Id);
    }

    public async Task<RestaurantEmployee> UpdateAsync(RestaurantEmployee employee)
    {
        _context.Entry(employee).State = EntityState.Modified;
        
        try
        {
            await _context.SaveChangesAsync();
            return await GetByIdAsync(employee.Id);
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await EmployeeExistsAsync(employee.Id))
                return null;
            throw;
        }
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var employee = await _context.RestaurantEmployees.FindAsync(id);
        if (employee == null)
            return false;

        _context.RestaurantEmployees.Remove(employee);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeactivateAsync(int id)
    {
        var employee = await _context.RestaurantEmployees.FindAsync(id);
        if (employee == null)
            return false;
        
        employee.IsActive = false;
        await _context.SaveChangesAsync();
        return true;
    }

    private async Task<bool> EmployeeExistsAsync(int id)
    {
        return await _context.RestaurantEmployees.AnyAsync(e => e.Id == id);
    }
}