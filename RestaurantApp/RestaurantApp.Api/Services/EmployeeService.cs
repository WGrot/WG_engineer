using Microsoft.EntityFrameworkCore;
using RestaurantApp.Api.Common;
using RestaurantApp.Api.Mappers;
using RestaurantApp.Api.Services.Interfaces;
using RestaurantApp.Domain.Models;
using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.DTOs;
using RestaurantApp.Shared.DTOs.Employees;
using RestaurantApp.Shared.Models;

namespace RestaurantApp.Api.Services;

public class EmployeeService : IEmployeeService
{
    private readonly ApiDbContext _context;
    private readonly IRestaurantService _restaurantService;

    public EmployeeService(ApiDbContext context, IRestaurantService restaurantService)
    {
        _context = context;
        _restaurantService = restaurantService;
    }

    public async Task<Result<IEnumerable<RestaurantEmployee>>> GetAllAsync()
    {
        var employees = await _context.RestaurantEmployees
            .Include(e => e.Restaurant)
            .Include(e => e.Permissions)
            .ToListAsync();

        return Result<IEnumerable<RestaurantEmployee>>.Success(employees);
    }

    public async Task<Result<RestaurantEmployee>> GetByIdAsync(int id)
    {
        var employee = await _context.RestaurantEmployees
            .Include(e => e.Restaurant)
            .Include(e => e.Permissions)
            .FirstOrDefaultAsync(e => e.Id == id);

        return employee == null
            ? Result<RestaurantEmployee>.NotFound($"Employee with ID {id} not found.")
            : Result.Success(employee);
    }

    public async Task<Result<IEnumerable<RestaurantEmployee>>> GetByRestaurantIdAsync(int restaurantId)
    {
        var employees = await _context.RestaurantEmployees
            .Include(e => e.Restaurant)
            .Include(e => e.Permissions)
            .Where(e => e.RestaurantId == restaurantId)
            .ToListAsync();

        return Result<IEnumerable<RestaurantEmployee>>.Success(employees);
    }

    public async Task<Result<IEnumerable<RestaurantEmployee>>> GetByUserIdAsync(string userId)
    {
        var employees = await _context.RestaurantEmployees
            .Include(e => e.Restaurant)
            .Include(e => e.Permissions)
            .Where(e => e.UserId == userId)
            .ToListAsync();

        return Result<IEnumerable<RestaurantEmployee>>.Success(employees);
    }

    public async Task<Result<IEnumerable<RestaurantEmployeeDto>>> GetEmployeesByRestaurantDtoAsync(int restaurantId)
    {
        var employees = await _context.RestaurantEmployees
            .Include(e => e.Restaurant)
            .Include(e => e.Permissions)
            .Where(e => e.RestaurantId == restaurantId)
            .ToListAsync();

        var dtoList = new List<RestaurantEmployeeDto>();

        foreach (var employee in employees)
        {
            var user = await _context.Users.FindAsync(employee.UserId);
            if (user == null)
                return Result.Failure<IEnumerable<RestaurantEmployeeDto>>(
                    $"User with ID {employee.UserId} not found", 404
                );

            var dto = new RestaurantEmployeeDto
            {
                Id = employee.Id,
                UserId = employee.UserId,
                RestaurantId = employee.RestaurantId,
                Restaurant = employee.Restaurant.ToDto(),
                Role = employee.Role,
                Permissions = employee.Permissions.ToDtoList(),
                CreatedAt = employee.CreatedAt,
                IsActive = employee.IsActive,
                Email = user.Email ?? string.Empty,
                FirstName = user.FirstName ?? string.Empty,
                LastName = user.LastName ?? string.Empty,
                PhoneNumber = user.PhoneNumber ?? string.Empty
            };

            dtoList.Add(dto);
        }

        return Result<IEnumerable<RestaurantEmployeeDto>>.Success(dtoList);
    }

    public async Task<Result> UpdateEmployeeRoleAsync(int employeeId, RestaurantRole newRole)
    {
        var employee = _context.RestaurantEmployees.Find(employeeId);
        if (employee == null)
        {
            return Result.NotFound($"Employee with ID {employeeId} not found.");
        }
        employee.Role = newRole;
        await _context.SaveChangesAsync();
        return Result.Success();
    }

    public async Task<Result<RestaurantEmployee>> CreateAsync(CreateEmployeeDto dto)
    {
        var restaurantResult = await _restaurantService.GetByIdAsync(dto.RestaurantId);
        if (restaurantResult.IsFailure)
            return Result<RestaurantEmployee>.Failure(restaurantResult.Error);

        var employee = new RestaurantEmployee
        {
            UserId = dto.UserId,
            RestaurantId = dto.RestaurantId,
            Role = dto.Role,
            Permissions = new List<RestaurantPermission>(),
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        _context.RestaurantEmployees.Add(employee);
        await _context.SaveChangesAsync();

        return Result.Success(employee);
    }

    public async Task<Result<RestaurantEmployee>> UpdateAsync(int id, UpdateEmployeeDto dto)
    {
        var employee = await _context.RestaurantEmployees.FindAsync(id);
        if (employee == null)
            return Result<RestaurantEmployee>.NotFound($"Employee with ID {id} not found.");

        employee.Role = dto.Role;
        employee.IsActive = dto.IsActive;
        // inne pola do aktualizacji

        await _context.SaveChangesAsync();
        return Result.Success(employee);
    }

    public async Task<Result> DeleteAsync(int id)
    {
        var employee = await _context.RestaurantEmployees.FindAsync(id);
        if (employee == null)
            return Result.NotFound($"Employee with ID {id} not found.");

        _context.RestaurantEmployees.Remove(employee);
        await _context.SaveChangesAsync();
        return Result.Success();
    }

    public async Task<Result> UpdateActiveStatusAsync(int id, bool isActive)
    {
        var employee = await _context.RestaurantEmployees.FindAsync(id);
        if (employee == null)
            return Result.NotFound($"Employee with ID {id} not found.");

        employee.IsActive = isActive;
        await _context.SaveChangesAsync();
    
        return Result.Success();
    }
}