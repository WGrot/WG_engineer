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
            // Seats i Restaurant nie są mapowane do DTO
        };
    }

    // Z DTO na Entity
    public static Table ToEntity(this TableDto dto)
    {
        return new Table
        {
            Id = dto.Id,
            TableNumber = dto.TableNumber,
            Capacity = dto.Capacity,
            Location = dto.Location,
            RestaurantId = dto.RestaurantId,
            Seats = new List<Seat>()
            // Restaurant będzie zarządzany przez EF
        };
    }

    // Aktualizacja istniejącej encji z DTO
    public static void UpdateFromDto(this Table entity, TableDto dto)
    {
        entity.TableNumber = dto.TableNumber;
        entity.Capacity = dto.Capacity;
        entity.Location = dto.Location;
        entity.RestaurantId = dto.RestaurantId;
        
        // Nie aktualizujemy Id, Seats ani relacji Restaurant
        // Seats są zarządzane osobno
    }

    // Mapowanie kolekcji Entity na listę DTO
    public static List<TableDto> ToDtoList(this IEnumerable<Table> entities)
    {
        return entities.Select(e => e.ToDto()).ToList();
    }

    // Mapowanie kolekcji DTO na listę Entity
    public static List<Table> ToEntityList(this IEnumerable<TableDto> dtos)
    {
        return dtos.Select(d => d.ToEntity()).ToList();
    }
}