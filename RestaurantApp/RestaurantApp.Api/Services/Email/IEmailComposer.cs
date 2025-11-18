namespace RestaurantApp.Api.Services.Email;

public interface IEmailComposer
{
    Task SendAsync(string to, IEmailTemplate template);
}