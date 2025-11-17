namespace RestaurantApp.Api.Services.Interfaces;

public interface IEmailService
{
    Task SendEmilAsync(string to, string subject, string body);
}