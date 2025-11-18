namespace RestaurantApp.Api.Services.Email;

public interface IEmailTemplate
{
    string Subject { get; }
    string BuildBody();
}