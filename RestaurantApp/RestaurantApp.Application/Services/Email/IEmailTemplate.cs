namespace RestaurantApp.Application.Services.Email;

public interface IEmailTemplate
{
    string Subject { get; }
    string BuildBody();
}