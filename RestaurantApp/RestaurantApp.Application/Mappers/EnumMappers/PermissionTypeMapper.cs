using DomainPermission = RestaurantApp.Domain.Enums.PermissionType;
using SharedPermission = RestaurantApp.Shared.Models.PermissionTypeEnumDto;

namespace RestaurantApp.Application.Mappers.EnumMappers;

public static class PermissionTypeMapper
{
    public static SharedPermission ToShared(this DomainPermission permission)
        => (SharedPermission)(int)permission;
    public static DomainPermission ToDomain(this SharedPermission permission)
        => (DomainPermission)(int)permission;

    public static IEnumerable<SharedPermission> ToShared(this IEnumerable<DomainPermission> permissions)
        => permissions.Select(p => p.ToShared());

    public static IEnumerable<DomainPermission> ToDomain(this IEnumerable<SharedPermission> permissions)
        => permissions.Select(p => p.ToDomain());
}