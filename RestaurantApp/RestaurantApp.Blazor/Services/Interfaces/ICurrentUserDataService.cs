using RestaurantApp.Shared.DTOs.Users;

namespace RestaurantApp.Blazor.Services.Interfaces;

public interface ICurrentUserDataService
{
    
    event Func<Task>? OnActiveRestaurantChanged;
    public Task<ResponseUserLoginDto?> GetUser();
    public Task SetUser(ResponseUserLoginDto user);
    public Task<string?> GetActiveRestaurant();
    public Task SetActiveRestaurant(string restaurantId);
    public Task Clear();
    Task RemoveActiveRestaurantAsync();
    Task RemoveUserAsync();
    Task NotifyActiveRestaurantChanged();

}