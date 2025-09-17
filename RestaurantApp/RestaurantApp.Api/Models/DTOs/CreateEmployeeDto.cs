using RestaurantApp.Shared.Models;

namespace RestaurantApp.Api.Models.DTOs;

public class CreateEmployeeDto
{
    public int Id { get; set; }
    public string UserId { get; set; }
    public int RestaurantId { get; set; }
    public RestaurantRole Role { get; set; }
}