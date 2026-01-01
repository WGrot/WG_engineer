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

    public async Task<Result<TableDto>> CreateTableAsync(CreateTableDto dto, CancellationToken ct)
    {
        if (!await AuthorizeForRestaurant(dto.RestaurantId, ct))
            return Result<TableDto>.Forbidden("You dont have permission to create tables for this restaurant.");
        
        return await _inner.CreateTableAsync(dto, ct);
    }

    public async Task<Result> UpdateTableAsync(int id, UpdateTableDto dto, CancellationToken ct)
    {
        if (!await AuthorizeForTable(id, ct))
            return Result.Forbidden("You dont have permission to create categories for this restaurant.");
        
        return await _inner.UpdateTableAsync(id, dto, ct);
    }

    public async Task<Result> UpdateTableCapacityAsync(int id, int capacity, CancellationToken ct)
    {
        if (!await AuthorizeForTable(id, ct))
            return Result.Forbidden("You dont have permission to create categories for this restaurant.");
        
        return await _inner.UpdateTableCapacityAsync(id, capacity, ct);
    }

    public async Task<Result> DeleteTableAsync(int id, CancellationToken ct)
    {
        if (!await AuthorizeForTable(id, ct))
            return Result.Forbidden("You dont have permission to create categories for this restaurant.");
        
        return await _inner.DeleteTableAsync(id, ct);
    }

    public async Task<Result<IEnumerable<TableDto>>> GetAllTablesAsync(CancellationToken ct)
    {
        return await _inner.GetAllTablesAsync(ct);
    }

    public async Task<Result<TableDto>> GetTableByIdAsync(int id, CancellationToken ct)
    {
        return await _inner.GetTableByIdAsync(id, ct);
    }

    public async Task<Result<IEnumerable<TableDto>>> GetTablesByRestaurantAsync(int restaurantId, CancellationToken ct)
    {
        return await _inner.GetTablesByRestaurantAsync(restaurantId, ct);
    }

    public async Task<Result<IEnumerable<TableDto>>> GetAvailableTablesAsync(int? minCapacity, CancellationToken ct)
    {
        return await _inner.GetAvailableTablesAsync(minCapacity, ct);
    }
    
    private async Task<bool> AuthorizeForTable(int tableId, CancellationToken ct)
    {
        if (!_currentUser.IsAuthenticated)
            return false;

        return await _authorizationChecker.CanManageTableAsync(_currentUser.UserId!, tableId);
    }
    
    private async Task<bool> AuthorizeForRestaurant(int tableId, CancellationToken ct)
    {
        if (!_currentUser.IsAuthenticated)
            return false;

        return await _authorizationChecker.HasPermissionInRestaurantAsync(_currentUser.UserId!, tableId, PermissionType.ManageTables);
    }
}