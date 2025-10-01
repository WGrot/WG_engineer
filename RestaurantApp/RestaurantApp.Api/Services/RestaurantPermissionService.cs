using Microsoft.EntityFrameworkCore;
using RestaurantApp.Api.Common;
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

    public async Task<Result<IEnumerable<RestaurantPermission>>> GetAllAsync()
    {
        try
        {
            var permissions = await _context.RestaurantPermissions
                .Include(p => p.RestaurantEmployee)
                .ToListAsync();
                
            return Result<IEnumerable<RestaurantPermission>>.Success(permissions);
        }
        catch (Exception ex)
        {
            return Result<IEnumerable<RestaurantPermission>>.InternalError($"Błąd podczas pobierania uprawnień: {ex.Message}");
        }
    }

    public async Task<Result<RestaurantPermission>> GetByIdAsync(int id)
    {
        try
        {
            var permission = await _context.RestaurantPermissions
                .Include(p => p.RestaurantEmployee)
                .FirstOrDefaultAsync(p => p.Id == id);
                
            if (permission == null)
                return Result<RestaurantPermission>.NotFound($"Nie znaleziono uprawnienia o ID: {id}");
                
            return Result<RestaurantPermission>.Success(permission);
        }
        catch (Exception ex)
        {
            return Result<RestaurantPermission>.InternalError($"Błąd podczas pobierania uprawnienia: {ex.Message}");
        }
    }

    public async Task<Result<IEnumerable<RestaurantPermission>>> GetByEmployeeIdAsync(int employeeId)
    {
        try
        {
            var permissions = await _context.RestaurantPermissions
                .Include(p => p.RestaurantEmployee)
                .Where(p => p.RestaurantEmployeeId == employeeId)
                .ToListAsync();
                
            return Result<IEnumerable<RestaurantPermission>>.Success(permissions);
        }
        catch (Exception ex)
        {
            return Result<IEnumerable<RestaurantPermission>>.InternalError($"Błąd podczas pobierania uprawnień pracownika: {ex.Message}");
        }
    }

    public async Task<Result<IEnumerable<RestaurantPermission>>> GetByRestaurantIdAsync(int restaurantId)
    {
        try
        {
            var permissions = await _context.RestaurantPermissions
                .Include(p => p.RestaurantEmployee)
                .Where(p => p.RestaurantEmployee.RestaurantId == restaurantId)
                .ToListAsync();
                
            return Result<IEnumerable<RestaurantPermission>>.Success(permissions);
        }
        catch (Exception ex)
        {
            return Result<IEnumerable<RestaurantPermission>>.InternalError($"Błąd podczas pobierania uprawnień restauracji: {ex.Message}");
        }
    }

    public async Task<Result<RestaurantPermission>> CreateAsync(RestaurantPermission permission)
    {
        try
        {
            // Sprawdzenie, czy pracownik istnieje
            var employeeExists = await _context.RestaurantEmployees
                .AnyAsync(e => e.Id == permission.RestaurantEmployeeId);
                
            if (!employeeExists)
                return Result<RestaurantPermission>.NotFound($"Nie znaleziono pracownika o ID: {permission.RestaurantEmployeeId}");
            
            // Sprawdzenie, czy takie uprawnienie już istnieje
            var existingPermission = await _context.RestaurantPermissions
                .AnyAsync(p => p.RestaurantEmployeeId == permission.RestaurantEmployeeId 
                    && p.Permission == permission.Permission);
                    
            if (existingPermission)
                return Result<RestaurantPermission>.Conflict($"Pracownik już posiada uprawnienie: {permission.Permission}");
            
            _context.RestaurantPermissions.Add(permission);
            await _context.SaveChangesAsync();
            
            var createdPermission = await _context.RestaurantPermissions
                .Include(p => p.RestaurantEmployee)
                .FirstOrDefaultAsync(p => p.Id == permission.Id);
                
            return Result<RestaurantPermission>.Created(createdPermission!);
        }
        catch (Exception ex)
        {
            return Result<RestaurantPermission>.InternalError($"Błąd podczas tworzenia uprawnienia: {ex.Message}");
        }
    }

    public async Task<Result<RestaurantPermission>> UpdateAsync(RestaurantPermission permission)
    {
        try
        {
            // Sprawdzenie, czy uprawnienie istnieje
            var existingPermission = await _context.RestaurantPermissions
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == permission.Id);
                
            if (existingPermission == null)
                return Result<RestaurantPermission>.NotFound($"Nie znaleziono uprawnienia o ID: {permission.Id}");
            
            // Sprawdzenie, czy pracownik istnieje
            var employeeExists = await _context.RestaurantEmployees
                .AnyAsync(e => e.Id == permission.RestaurantEmployeeId);
                
            if (!employeeExists)
                return Result<RestaurantPermission>.NotFound($"Nie znaleziono pracownika o ID: {permission.RestaurantEmployeeId}");
            
            // Sprawdzenie duplikatu (jeśli zmieniono typ uprawnienia)
            if (existingPermission.Permission != permission.Permission)
            {
                var duplicateExists = await _context.RestaurantPermissions
                    .AnyAsync(p => p.RestaurantEmployeeId == permission.RestaurantEmployeeId 
                        && p.Permission == permission.Permission 
                        && p.Id != permission.Id);
                        
                if (duplicateExists)
                    return Result<RestaurantPermission>.Conflict($"Pracownik już posiada uprawnienie: {permission.Permission}");
            }
            
            _context.Entry(permission).State = EntityState.Modified;
            
            await _context.SaveChangesAsync();
            
            var updatedPermission = await _context.RestaurantPermissions
                .Include(p => p.RestaurantEmployee)
                .FirstOrDefaultAsync(p => p.Id == permission.Id);
                
            return Result<RestaurantPermission>.Success(updatedPermission!);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Result<RestaurantPermission>.Conflict("Uprawnienie zostało zmodyfikowane przez innego użytkownika");
        }
        catch (Exception ex)
        {
            return Result<RestaurantPermission>.InternalError($"Błąd podczas aktualizacji uprawnienia: {ex.Message}");
        }
    }

    public async Task<Result> DeleteAsync(int id)
    {
        try
        {
            var permission = await _context.RestaurantPermissions.FindAsync(id);
            if (permission == null)
                return Result.NotFound($"Nie znaleziono uprawnienia o ID: {id}");

            _context.RestaurantPermissions.Remove(permission);
            await _context.SaveChangesAsync();
            
            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.InternalError($"Błąd podczas usuwania uprawnienia: {ex.Message}");
        }
    }

    public async Task<Result<bool>> HasPermissionAsync(int employeeId, PermissionType permission)
    {
        try
        {
            var hasPermission = await _context.RestaurantPermissions
                .AnyAsync(p => p.RestaurantEmployeeId == employeeId && p.Permission == permission);
                
            return Result<bool>.Success(hasPermission);
        }
        catch (Exception ex)
        {
            return Result<bool>.InternalError($"Błąd podczas sprawdzania uprawnienia: {ex.Message}");
        }
    }
}