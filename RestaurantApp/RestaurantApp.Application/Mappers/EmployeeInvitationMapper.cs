using RestaurantApp.Application.Mappers.EnumMappers;
using RestaurantApp.Domain.Enums;
using RestaurantApp.Domain.Models;
using RestaurantApp.Shared.DTOs.Employees;
using RestaurantApp.Shared.Models;

namespace RestaurantApp.Application.Mappers;

public static class EmployeeInvitationMapper
{
    public static EmployeeInvitationDto ToDto(this EmployeeInvitation invitation)
    {
        return new EmployeeInvitationDto
        {
            Id = invitation.Id,
            RestaurantId = invitation.RestaurantId,
            UserId = invitation.UserId,
            Role = invitation.Role.ToShared(),
            Status = invitation.Status.ToShared(),
            CreatedAt = invitation.CreatedAt,
            ExpiresAt = invitation.ExpiresAt,
            RespondedAt = invitation.RespondedAt
        };
    }

    public static EmployeeInvitation MapToEntity(this EmployeeInvitationDto dto)
    {
        return new EmployeeInvitation
        {
            Id = dto.Id,
            RestaurantId = dto.RestaurantId,
            UserId = dto.UserId,
            Role = dto.Role.ToDomain(),
            Status = dto.Status.ToDomain(),
            CreatedAt = dto.CreatedAt,
            ExpiresAt = dto.ExpiresAt,
            RespondedAt = dto.RespondedAt
        };
    }

    public static List<EmployeeInvitationDto> ToDtoList(this IEnumerable<EmployeeInvitation> entities)
    {
        return entities.Select(e => e.ToDto()).ToList();
    }

    public static List<EmployeeInvitation> ToEntityList(this IEnumerable<EmployeeInvitationDto> dtos)
    {
        return dtos.Select(d => d.MapToEntity()).ToList();
    }

    // InvitationStatus mapping
    public static InvitationStatusEnumDto ToShared(this InvitationStatus status)
        => (InvitationStatusEnumDto)(int)status;

    public static InvitationStatus ToDomain(this InvitationStatusEnumDto status)
        => (InvitationStatus)(int)status;

    public static IEnumerable<InvitationStatusEnumDto> ToShared(this IEnumerable<InvitationStatus> statuses)
        => statuses.Select(s => s.ToShared());

    public static IEnumerable<InvitationStatus> ToDomain(this IEnumerable<InvitationStatusEnumDto> statuses)
        => statuses.Select(s => s.ToDomain());


}