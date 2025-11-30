using DomainRestaurantRole = RestaurantApp.Domain.Enums.RestaurantRole;
using SharedRestaurantRole = RestaurantApp.Shared.Models.RestaurantRoleEnumDto;

namespace RestaurantApp.Application.Mappers.EnumMappers;

public static class RestaurantRoleMapper
{
    public static SharedRestaurantRole ToShared(this DomainRestaurantRole role)
        => (SharedRestaurantRole)(int)role;

    public static DomainRestaurantRole ToDomain(this SharedRestaurantRole role)
        => (DomainRestaurantRole)(int)role;

    public static IEnumerable<SharedRestaurantRole> ToShared(this IEnumerable<DomainRestaurantRole> roles)
        => roles.Select(r => r.ToShared());

    public static IEnumerable<DomainRestaurantRole> ToDomain(this IEnumerable<SharedRestaurantRole> roles)
        => roles.Select(r => r.ToDomain());
}