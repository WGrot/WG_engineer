namespace RestaurantApp.Shared.Models;

public class RestaurantPermission
{
    public int Id { get; set; }
    public int RestaurantEmployeeId { get; set; }
    public RestaurantEmployee RestaurantEmployee { get; set; }
    public PermissionType Permission { get; set; }
}
