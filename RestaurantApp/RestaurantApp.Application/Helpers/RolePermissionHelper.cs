using RestaurantApp.Domain.Enums;
using RestaurantApp.Domain.Models;

namespace RestaurantApp.Application.Helpers;

public static class RolePermissionHelper
{
    public static List<PermissionType> GetDefaultPermissions(RestaurantRole role)
    {
        switch (role)
        {
            case RestaurantRole.Owner:
                return new List<PermissionType>
                {
                    PermissionType.ManageRestaurant,
                    PermissionType.ManageReservations,
                    PermissionType.ManageTables,
                    PermissionType.ManageMenu,
                    PermissionType.ManageEmployees,
                    PermissionType.ManagePermissions,
                    PermissionType.ManageRestaurantSettings
                };
            case RestaurantRole.Manager:
                return new List<PermissionType>
                {
                    PermissionType.ManageReservations,
                    PermissionType.ManageTables,
                    PermissionType.ManageMenu,
                    PermissionType.ManageEmployees,
                    PermissionType.ManagePermissions,
                    PermissionType.ManageRestaurantSettings
                };
            case RestaurantRole.Employee:
                return new List<PermissionType>
                {
                    PermissionType.ManageReservations,
                };
            case RestaurantRole.Chef:
                return new List<PermissionType>
                {
                    PermissionType.ManageMenu,
                    PermissionType.ManageReservations,
                };
            case RestaurantRole.Waiter:
                return new List<PermissionType>
                {
                    PermissionType.ManageReservations,
                    PermissionType.ManageTables,
                };
            default:
                return new List<PermissionType>
                {
                    PermissionType.ManageReservations
                };
        }
    }
}