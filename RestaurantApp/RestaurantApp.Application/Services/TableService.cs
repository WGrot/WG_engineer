using RestaurantApp.Application.Interfaces.Repositories;
using RestaurantApp.Application.Interfaces.Services;
using RestaurantApp.Application.Mappers;
using RestaurantApp.Domain.Models;
using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.DTOs.Tables;

namespace RestaurantApp.Application.Services;

public class TableService :ITableService
{
    private readonly ITableRepository _tableRepository;
    private readonly IRestaurantRepository _restaurantRepository;

    public TableService(ITableRepository tableRepository, IRestaurantRepository restaurantRepository)
    {
        _tableRepository = tableRepository;
        _restaurantRepository = restaurantRepository;
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
        var restaurantExists = await _restaurantRepository.ExistsAsync(dto.RestaurantId);
        if (!restaurantExists)
            return Result<TableDto>.ValidationError($"Restaurant with ID {dto.RestaurantId} does not exist.");

        var tableNumberExists = await _tableRepository.TableNumberExistsInRestaurantAsync(
            dto.TableNumber, dto.RestaurantId);

        if (tableNumberExists)
            return Result<TableDto>.Conflict($"Table number {dto.TableNumber} already exists in this restaurant.");

        var table = CreateTableEntity(dto);

        var createdTable = await _tableRepository.AddAsync(table);
        return Result<TableDto>.Created(createdTable.ToDto());
    }

    public async Task<Result> UpdateTableAsync(int id, UpdateTableDto dto)
    {
        var table = await _tableRepository.GetByIdAsync(id);
        if (table is null)
            return Result.NotFound($"Table with ID {id} not found.");

        if (table.TableNumber != dto.TableNumber)
        {
            var exists = await _tableRepository.TableNumberExistsInRestaurantAsync(
                dto.TableNumber, table.RestaurantId, excludeTableId: id);

            if (exists)
                return Result.Conflict($"Table number {dto.TableNumber} already exists in this restaurant.");
        }

        table.TableNumber = dto.TableNumber;
        table.Capacity = dto.Capacity;
        table.Location = dto.Location;

        try
        {
            await _tableRepository.UpdateAsync(table);
        }
        catch (Exception ex)
        {
            return Result.InternalError("Concurrency error while updating table.");
        }

        return Result.Success();
    }

    public async Task<Result> UpdateTableCapacityAsync(int id, int capacity)
    {
        if (capacity <= 0)
            return Result.ValidationError("Capacity must be greater than 0.");

        var table = await _tableRepository.GetByIdAsync(id);
        if (table is null)
            return Result.NotFound($"Table with ID {id} not found.");

        table.Capacity = capacity;
        await _tableRepository.UpdateAsync(table);

        return Result.Success();
    }

    public async Task<Result> DeleteTableAsync(int id)
    {
        var table = await _tableRepository.GetByIdWithRestaurantAndSeatsAsync(id);
        if (table is null)
            return Result.NotFound($"Table with ID {id} not found.");

        await _tableRepository.DeleteAsync(table);
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