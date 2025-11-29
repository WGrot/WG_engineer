using Microsoft.EntityFrameworkCore;
using RestaurantApp.Api.Common;
using RestaurantApp.Api.Mappers;
using RestaurantApp.Api.Services.Interfaces;
using RestaurantApp.Domain.Models;
using RestaurantApp.Infrastructure.Persistence;
using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.DTOs;
using RestaurantApp.Shared.DTOs.Permissions;
using RestaurantApp.Shared.Models;

namespace RestaurantApp.Api.Services;

public class RestaurantPermissionService : IRestaurantPermissionService
{
    private readonly ApplicationDbContext _context;

    public RestaurantPermissionService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<IEnumerable<RestaurantPermissionDto>>> GetAllAsync()
    {
        try
        {
            var permissions = await _context.RestaurantPermissions
                .Include(p => p.RestaurantEmployee)
                .ToListAsync();

            return Result<IEnumerable<RestaurantPermissionDto>>.Success(permissions.ToDtoList());
        }
        catch (Exception ex)
        {
            return Result<IEnumerable<RestaurantPermissionDto>>.InternalError(
                $"Błąd podczas pobierania uprawnień: {ex.Message}");
        }
    }

    public async Task<Result<RestaurantPermissionDto>> GetByIdAsync(int id)
    {
        try
        {
            var permission = await _context.RestaurantPermissions
                .Include(p => p.RestaurantEmployee)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (permission == null)
                return Result<RestaurantPermissionDto>.NotFound($"Nie znaleziono uprawnienia o ID: {id}");

            return Result<RestaurantPermissionDto>.Success(permission.ToDto());
        }
        catch (Exception ex)
        {
            return Result<RestaurantPermissionDto>.InternalError($"Błąd podczas pobierania uprawnienia: {ex.Message}");
        }
    }

    public async Task<Result<IEnumerable<RestaurantPermissionDto>>> GetByEmployeeIdAsync(int employeeId)
    {
        try
        {
            var permissions = await _context.RestaurantPermissions
                .Include(p => p.RestaurantEmployee)
                .Where(p => p.RestaurantEmployeeId == employeeId)
                .ToListAsync();

            return Result<IEnumerable<RestaurantPermissionDto>>.Success(permissions.ToDtoList());
        }
        catch (Exception ex)
        {
            return Result<IEnumerable<RestaurantPermissionDto>>.InternalError(
                $"Błąd podczas pobierania uprawnień pracownika: {ex.Message}");
        }
    }

    public async Task<Result<IEnumerable<RestaurantPermissionDto>>> GetByRestaurantIdAsync(int restaurantId)
    {
        try
        {
            var permissions = await _context.RestaurantPermissions
                .Include(p => p.RestaurantEmployee)
                .Where(p => p.RestaurantEmployee.RestaurantId == restaurantId)
                .ToListAsync();

            return Result<IEnumerable<RestaurantPermissionDto>>.Success(permissions.ToDtoList());
        }
        catch (Exception ex)
        {
            return Result<IEnumerable<RestaurantPermissionDto>>.InternalError(
                $"Błąd podczas pobierania uprawnień restauracji: {ex.Message}");
        }
    }

    public async Task<Result<RestaurantPermissionDto>> CreateAsync(CreateRestaurantPermissionDto dto)
    {
        try
        {
            // Sprawdzenie, czy pracownik istnieje
            var employeeExists = await _context.RestaurantEmployees
                .AnyAsync(e => e.Id == dto.RestaurantEmployeeId);

            if (!employeeExists)
                return Result<RestaurantPermissionDto>.NotFound(
                    $"Nie znaleziono pracownika o ID: {dto.RestaurantEmployeeId}");

            // Sprawdzenie, czy takie uprawnienie już istnieje
            var existingPermission = await _context.RestaurantPermissions
                .AnyAsync(p => p.RestaurantEmployeeId == dto.RestaurantEmployeeId
                               && p.Permission == dto.Permission);

            if (existingPermission)
                return Result<RestaurantPermissionDto>.Conflict(
                    $"Pracownik już posiada uprawnienie: {dto.Permission}");

            var permission = dto.ToEntity();
            _context.RestaurantPermissions.Add(permission);
            await _context.SaveChangesAsync();


            return Result<RestaurantPermissionDto>.Created(permission.ToDto());
        }
        catch (Exception ex)
        {
            return Result<RestaurantPermissionDto>.InternalError($"Błąd podczas tworzenia uprawnienia: {ex.Message}");
        }
    }

    public async Task<Result<RestaurantPermissionDto>> UpdateAsync(RestaurantPermissionDto permission)
    {
        try
        {
            // Sprawdzenie, czy uprawnienie istnieje
            var existingPermission = await _context.RestaurantPermissions
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == permission.Id);

            if (existingPermission == null)
                return Result<RestaurantPermissionDto>.NotFound($"Nie znaleziono uprawnienia o ID: {permission.Id}");

            // Sprawdzenie, czy pracownik istnieje
            var employeeExists = await _context.RestaurantEmployees
                .AnyAsync(e => e.Id == permission.RestaurantEmployeeId);

            if (!employeeExists)
                return Result<RestaurantPermissionDto>.NotFound(
                    $"Nie znaleziono pracownika o ID: {permission.RestaurantEmployeeId}");

            // Sprawdzenie duplikatu (jeśli zmieniono typ uprawnienia)
            if (existingPermission.Permission != permission.Permission)
            {
                var duplicateExists = await _context.RestaurantPermissions
                    .AnyAsync(p => p.RestaurantEmployeeId == permission.RestaurantEmployeeId
                                   && p.Permission == permission.Permission
                                   && p.Id != permission.Id);

                if (duplicateExists)
                    return Result<RestaurantPermissionDto>.Conflict(
                        $"Pracownik już posiada uprawnienie: {permission.Permission}");
            }

            _context.RestaurantPermissions.Update(permission.ToEntity());

            await _context.SaveChangesAsync();

            var updatedPermission = await _context.RestaurantPermissions
                .Include(p => p.RestaurantEmployee)
                .FirstOrDefaultAsync(p => p.Id == permission.Id);

            return Result<RestaurantPermissionDto>.Success(updatedPermission.ToDto());
        }
        catch (DbUpdateConcurrencyException)
        {
            return Result<RestaurantPermissionDto>.Conflict("Uprawnienie zostało zmodyfikowane przez innego użytkownika");
        }
        catch (Exception ex)
        {
            return Result<RestaurantPermissionDto>.InternalError($"Błąd podczas aktualizacji uprawnienia: {ex.Message}");
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

    public async Task<Result<int?>> HasPermissionAsync(int employeeId, PermissionType permission)
    {
        try
        {
            var permissionId = await _context.RestaurantPermissions
                .Where(p => p.RestaurantEmployeeId == employeeId && p.Permission == permission)
                .Select(p => (int?)p.Id)
                .FirstOrDefaultAsync();

            return Result<int?>.Success(permissionId); // null jeśli nie istnieje
        }
        catch (Exception ex)
        {
            return Result<int?>.InternalError($"Błąd podczas sprawdzania uprawnienia: {ex.Message}");
        }
    }

    public async Task<Result> UpdateEmployeePermisions(UpdateEmployeePermisionsDto dto)
    {
        Result<RestaurantPermissionDto> result;
        foreach (PermissionType permission in dto.Permissions)
        {
            Result<int?> PermissionExist = await HasPermissionAsync(dto.EmployeeId, permission);

            if (PermissionExist.Value != null)
            {
                var newPermission = new RestaurantPermission
                {
                    Id = PermissionExist.Value.Value,
                    RestaurantEmployeeId = dto.EmployeeId,
                    Permission = permission,
                };
                result = await UpdateAsync(newPermission.ToDto());

            }
            else
            {
                CreateRestaurantPermissionDto createPermission = new CreateRestaurantPermissionDto
                {
                    RestaurantEmployeeId = dto.EmployeeId,
                    Permission = permission,
                };
                result = await CreateAsync(createPermission);
            }

            if (result.IsFailure)
            {
                return Result.Failure(result.Error);
            }
        }
        
        var permissionsResponse = (await GetByEmployeeIdAsync(dto.EmployeeId)).Value;
        
        if(permissionsResponse == null)
            return Result.Success();
        
        foreach (RestaurantPermissionDto permission in permissionsResponse)
        {
            if (!dto.Permissions.Contains(permission.Permission))
            {
               await DeleteAsync(permission.Id);
            }
                 
        }

        return Result.Success();
    }
}