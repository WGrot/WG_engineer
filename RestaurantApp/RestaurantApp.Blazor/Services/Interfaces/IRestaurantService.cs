namespace RestaurantApp.Blazor.Services.Interfaces;

public interface IRestaurantService
{
    Task<List<(int Id, string Name)>> GetRestaurantNames();
}