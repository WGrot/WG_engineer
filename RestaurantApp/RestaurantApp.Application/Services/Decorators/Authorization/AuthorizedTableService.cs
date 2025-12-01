using RestaurantApp.Application.Interfaces;
using RestaurantApp.Application.Interfaces.Services;
using RestaurantApp.Domain.Enums;
using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.DTOs.Tables;

namespace RestaurantApp.Application.Services.Decorators.Authorization;

public class AuthorizedTableService : ITableService
{
    private readonly ITableService _inner;
    private readonly ICurrentUserService _currentUser;
    private readonly IAuthorizationChecker _authorizationChecker;

    public AuthorizedTableService(
        ITableService inner,
        ICurrentUserService currentUser,
        IAuthorizationChecker authorizationChecker)
    {
        _inner = inner;
        _currentUser = currentUser;
        _authorizationChecker = authorizationChecker;
    }

    public async Task<Result<TableDto>> CreateTableAsync(CreateTableDto dto)
    {
        if (!await AuthorizeForRestaurant(dto.RestaurantId))
            return Result<TableDto>.Forbidden("You dont have permission to create categories for this restaurant.");
        
        return await _inner.CreateTableAsync(dto);
    }

    public async Task<Result> UpdateTableAsync(int id, UpdateTableDto dto)
    {
        if (!await AuthorizeForTable(id))
            return Result.Forbidden("You dont have permission to create categories for this restaurant.");
        
        return await _inner.UpdateTableAsync(id, dto);
    }

    public async Task<Result> UpdateTableCapacityAsync(int id, int capacity)
    {
        if (!await AuthorizeForTable(id))
            return Result.Forbidden("You dont have permission to create categories for this restaurant.");
        
        return await _inner.UpdateTableCapacityAsync(id, capacity);
    }

    public async Task<Result> DeleteTableAsync(int id)
    {
        if (!await AuthorizeForTable(id))
            return Result.Forbidden("You dont have permission to create categories for this restaurant.");
        
        return await _inner.DeleteTableAsync(id);
    }

    public async Task<Result<IEnumerable<TableDto>>> GetAllTablesAsync()
    {
        return await _inner.GetAllTablesAsync();
    }

    public async Task<Result<TableDto>> GetTableByIdAsync(int id)
    {
        return await _inner.GetTableByIdAsync(id);
    }

    public async Task<Result<IEnumerable<TableDto>>> GetTablesByRestaurantAsync(int restaurantId)
    {
        return await _inner.GetTablesByRestaurantAsync(restaurantId);
    }

    public async Task<Result<IEnumerable<TableDto>>> GetAvailableTablesAsync(int? minCapacity)
    {
        return await _inner.GetAvailableTablesAsync(minCapacity);
    }
    
    private async Task<bool> AuthorizeForTable(int tableId)
    {
        if (!_currentUser.IsAuthenticated)
            return false;

        return await _authorizationChecker.CanManageTableAsync(_currentUser.UserId!, tableId);
    }
    
    private async Task<bool> AuthorizeForRestaurant(int tableId)
    {
        if (!_currentUser.IsAuthenticated)
            return false;

        return await _authorizationChecker.HasPermissionInRestaurantAsync(_currentUser.UserId!, tableId, PermissionType.ManageTables);
    }
}