using RestaurantApp.Application.Interfaces.Repositories;
using RestaurantApp.Application.Interfaces.Services;
using RestaurantApp.Application.Mappers;
using RestaurantApp.Domain.Models;
using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.DTOs.Tables;

namespace RestaurantApp.Application.Services;

public class TableService : ITableService
{
    private readonly ITableRepository _tableRepository;

    public TableService(ITableRepository tableRepository)
    {
        _tableRepository = tableRepository;
    }

    public async Task<Result<IEnumerable<TableDto>>> GetAllTablesAsync(CancellationToken ct)
    {
        var tables = await _tableRepository.GetAllWithRestaurantAndSeatsAsync( ct);
        return Result.Success<IEnumerable<TableDto>>(tables.ToDtoList());
    }

    public async Task<Result<TableDto>> GetTableByIdAsync(int id, CancellationToken ct)
    {
        var table = await _tableRepository.GetByIdWithRestaurantAndSeatsAsync(id, ct);

        return table is null
            ? Result<TableDto>.NotFound($"Table with ID {id} not found.")
            : Result.Success(table.ToDto());
    }

    public async Task<Result<IEnumerable<TableDto>>> GetTablesByRestaurantAsync(int restaurantId, CancellationToken ct)
    {
        var tables = await _tableRepository.GetByRestaurantIdWithSeatsAsync(restaurantId, ct);
        return Result.Success<IEnumerable<TableDto>>(tables.ToDtoList());
    }

    public async Task<Result<IEnumerable<TableDto>>> GetAvailableTablesAsync(int? minCapacity, CancellationToken ct)
    {
        var tables = await _tableRepository.GetAvailableTablesAsync(minCapacity, ct);
        return Result.Success<IEnumerable<TableDto>>(tables.ToDtoList());
    }

    public async Task<Result<TableDto>> CreateTableAsync(CreateTableDto dto, CancellationToken ct)
    {
        var table = CreateTableEntity(dto);
        var createdTable = await _tableRepository.AddAsync(table, ct);
        return Result<TableDto>.Created(createdTable.ToDto());
    }

    public async Task<Result> UpdateTableAsync(int id, UpdateTableDto dto, CancellationToken ct)
    {
        var table = await _tableRepository.GetByIdAsync(id, ct);

        table!.TableNumber = dto.TableNumber;
        table.Capacity = dto.Capacity;
        table.Location = dto.Location;

        try
        {
            await _tableRepository.UpdateAsync(table, ct);
        }
        catch (Exception)
        {
            return Result.InternalError("Concurrency error while updating table.");
        }

        return Result.Success();
    }

    public async Task<Result> UpdateTableCapacityAsync(int id, int capacity, CancellationToken ct)
    {
        var table = await _tableRepository.GetByIdAsync(id, ct);

        table!.Capacity = capacity;
        await _tableRepository.UpdateAsync(table, ct);

        return Result.Success();
    }

    public async Task<Result> DeleteTableAsync(int id, CancellationToken ct)
    {
        var table = await _tableRepository.GetByIdWithRestaurantAndSeatsAsync(id, ct);

        await _tableRepository.DeleteAsync(table!, ct);
        return Result.Success();
    }

    private static Table CreateTableEntity(CreateTableDto dto)
    {
        var table = new Table
        {
            TableNumber = dto.TableNumber,
            Capacity = dto.Capacity,
            Location = dto.Location,
            RestaurantId = dto.RestaurantId,

        };

        return table;
    }
}