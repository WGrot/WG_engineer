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

    public async Task<Result<IEnumerable<TableDto>>> GetAllTablesAsync()
    {
        var tables = await _tableRepository.GetAllWithRestaurantAndSeatsAsync();
        return Result.Success<IEnumerable<TableDto>>(tables.ToDtoList());
    }

    public async Task<Result<TableDto>> GetTableByIdAsync(int id)
    {
        var table = await _tableRepository.GetByIdWithRestaurantAndSeatsAsync(id);

        return table is null
            ? Result<TableDto>.NotFound($"Table with ID {id} not found.")
            : Result.Success(table.ToDto());
    }

    public async Task<Result<IEnumerable<TableDto>>> GetTablesByRestaurantAsync(int restaurantId)
    {
        var tables = await _tableRepository.GetByRestaurantIdWithSeatsAsync(restaurantId);
        return Result.Success<IEnumerable<TableDto>>(tables.ToDtoList());
    }

    public async Task<Result<IEnumerable<TableDto>>> GetAvailableTablesAsync(int? minCapacity)
    {
        var tables = await _tableRepository.GetAvailableTablesAsync(minCapacity);
        return Result.Success<IEnumerable<TableDto>>(tables.ToDtoList());
    }

    public async Task<Result<TableDto>> CreateTableAsync(CreateTableDto dto)
    {
        var table = CreateTableEntity(dto);
        var createdTable = await _tableRepository.AddAsync(table);
        return Result<TableDto>.Created(createdTable.ToDto());
    }

    public async Task<Result> UpdateTableAsync(int id, UpdateTableDto dto)
    {
        var table = await _tableRepository.GetByIdAsync(id);

        table!.TableNumber = dto.TableNumber;
        table.Capacity = dto.Capacity;
        table.Location = dto.Location;

        await _tableRepository.UpdateAsync(table);
        return Result.Success();
    }

    public async Task<Result> UpdateTableCapacityAsync(int id, int capacity)
    {
        var table = await _tableRepository.GetByIdAsync(id);

        table!.Capacity = capacity;
        await _tableRepository.UpdateAsync(table);

        return Result.Success();
    }

    public async Task<Result> DeleteTableAsync(int id)
    {
        var table = await _tableRepository.GetByIdWithRestaurantAndSeatsAsync(id);

        await _tableRepository.DeleteAsync(table!);
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
            Seats = new List<Seat>()
        };

        if (dto.SeatCount > 0)
        {
            for (int i = 1; i <= dto.SeatCount; i++)
            {
                table.Seats.Add(new Seat
                {
                    SeatNumber = $"{table.TableNumber}-{i}",
                    IsAvailable = true,
                    Type = "Standard"
                });
            }
        }

        return table;
    }
}