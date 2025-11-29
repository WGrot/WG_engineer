using RestaurantApp.Application.Services.Email;

namespace RestaurantApp.Application.Interfaces;

public interface IEmailComposer
{
    Task SendAsync(string to, IEmailTemplate template);
}