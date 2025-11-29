using RestaurantApp.Domain.Models;
using RestaurantApp.Shared.DTOs.Employees;
using RestaurantApp.Shared.DTOs.Permissions;

namespace RestaurantApp.Application.Mappers;

public static class EmployeeMapper
{
    public static RestaurantEmployeeDto ToDto(this RestaurantEmployee entity)
    {
        return new RestaurantEmployeeDto
        {
            Id = entity.Id,
            UserId = entity.UserId,
            RestaurantId = entity.RestaurantId,
            Restaurant = entity.Restaurant?.ToDto(),
            Role = entity.Role,
            Permissions = entity.Permissions?.Select(p => p.ToDto()).ToList() ?? new List<RestaurantPermissionDto>(),
            CreatedAt = entity.CreatedAt,
            IsActive = entity.IsActive
            // Email, FirstName, LastName, PhoneNumber są tylko w DTO
            // Te wartości powinny być uzupełniane z innego źródła (np. Identity)
        };
    }

    // Z DTO na Entity
    public static RestaurantEmployee ToEntity(this RestaurantEmployeeDto dto)
    {
        return new RestaurantEmployee
        {
            Id = dto.Id,
            UserId = dto.UserId,
            RestaurantId = dto.RestaurantId,
            Role = dto.Role,
            Permissions = dto.Permissions?.Select(p => p.ToEntity()).ToList() ?? new List<RestaurantPermission>(),
            CreatedAt = dto.CreatedAt,
            IsActive = dto.IsActive
            // Restaurant nie jest mapowana - będzie zarządzana przez EF
            // Email, FirstName, LastName, PhoneNumber nie istnieją w Entity
        };
    }

    // Aktualizacja istniejącej encji z DTO
    public static void UpdateFromDto(this RestaurantEmployee entity, RestaurantEmployeeDto dto)
    {
        entity.UserId = dto.UserId;
        entity.RestaurantId = dto.RestaurantId;
        entity.Role = dto.Role;
        entity.IsActive = dto.IsActive;
        
        // Aktualizacja uprawnień (jeśli przekazane)
        if (dto.Permissions != null)
        {
            entity.Permissions = dto.Permissions.Select(p => p.ToEntity()).ToList();
        }
        
        // Nie aktualizujemy Id, CreatedAt ani relacji Restaurant
        // Email, FirstName, LastName, PhoneNumber są zarządzane osobno (Identity)
    }

    // Mapowanie kolekcji Entity na listę DTO
    public static List<RestaurantEmployeeDto> ToDtoList(this IEnumerable<RestaurantEmployee> entities)
    {
        return entities.Select(e => e.ToDto()).ToList();
    }

    // Mapowanie kolekcji DTO na listę Entity
    public static List<RestaurantEmployee> ToEntityList(this IEnumerable<RestaurantEmployeeDto> dtos)
    {
        return dtos.Select(d => d.ToEntity()).ToList();
    }
    
    // Opcjonalna metoda do uzupełnienia danych użytkownika w DTO
    public static void SetUserData(this RestaurantEmployeeDto dto, string email, string firstName, string lastName, string phoneNumber)
    {
        dto.Email = email;
        dto.FirstName = firstName;
        dto.LastName = lastName;
        dto.PhoneNumber = phoneNumber;
    }
}