namespace RestaurantApp.Shared.Models;

public class RestaurantPermission
{
    public int Id { get; set; }
    public int RestaurantEmployeeId { get; set; }
    public RestaurantEmployee RestaurantEmployee { get; set; }
    public PermissionType Permission { get; set; }
}

public enum PermissionType
{
    ViewReservations = 1,
    ManageReservations = 2,
    ManageTables = 3,
    ManageMenu = 4,
    ManageEmployees = 5,
    ViewReports = 6,
    ManageRestaurantSettings = 7,
    ManageFinances = 8
}