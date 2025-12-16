using RestaurantApp.Domain.Models;
using RestaurantApp.Shared.DTOs.Tables;

namespace RestaurantApp.Application.Mappers;

public static class TableMapper
{
    public static TableDto ToDto(this Table entity)
    {
        return new TableDto
        {
            Id = entity.Id,
            TableNumber = entity.TableNumber,
            Capacity = entity.Capacity,
            Location = entity.Location,
            RestaurantId = entity.RestaurantId

        };
    }
    
    public static Table ToEntity(this TableDto dto)
    {
        return new Table
        {
            Id = dto.Id,
            TableNumber = dto.TableNumber,
            Capacity = dto.Capacity,
            Location = dto.Location,
            RestaurantId = dto.RestaurantId,

        };
    }
    
    public static void UpdateFromDto(this Table entity, TableDto dto)
    {
        entity.TableNumber = dto.TableNumber;
        entity.Capacity = dto.Capacity;
        entity.Location = dto.Location;
        entity.RestaurantId = dto.RestaurantId;
    }


    public static List<TableDto> ToDtoList(this IEnumerable<Table> entities)
    {
        return entities.Select(e => e.ToDto()).ToList();
    }
    
    public static List<Table> ToEntityList(this IEnumerable<TableDto> dtos)
    {
        return dtos.Select(d => d.ToEntity()).ToList();
    }
}