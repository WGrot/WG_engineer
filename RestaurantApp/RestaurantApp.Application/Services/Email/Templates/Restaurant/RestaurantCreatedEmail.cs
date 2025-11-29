using RestaurantApp.Shared.DTOs.Restaurant;

namespace RestaurantApp.Application.Services.Email.Templates.Restaurant;

public class RestaurantCreatedEmail: IEmailTemplate
{
    private readonly string _username;
    private CreateRestaurantDto _dto;

    public RestaurantCreatedEmail(string username, CreateRestaurantDto dto)
    {
        _dto = dto;
        _username = username;
    }

    public string Subject => "Thank you for choosing DineOps";

    public string BuildBody()
    {
        return $@"
                <h2>Hello {_username}!</h2>
                <p>Your restaurant profile: {_dto.Name}</p>
                <p>Address: {_dto.Address} </p>
                <p>Have been created. Now you can customize and further configure your restaurant in Edit Page</p>
                <p>Thank you for choosing DineOps for your business</p>
            ";
    }
}