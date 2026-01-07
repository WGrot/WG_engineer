namespace RestaurantApp.Application.Services.Email.Templates.AccountManagement;

public class EmployeeInvitationEmail: IEmailTemplate
{
    private readonly string _username;
    private readonly string _acceptLink;
    private readonly string _role;
    private readonly string _restaurantName;

    public EmployeeInvitationEmail(string username, string acceptLink, string role, string restaurantName)
    {
        _acceptLink = acceptLink;
        _username = username;
        _role = role;
        _restaurantName = restaurantName;
    }

    public string Subject => "Employee Invitation to DineOps";

    public string BuildBody()
    {
        return $@"
            <h2>Hello {_username}!</h2>
            <p>You have been invited to work in restaurant {_restaurantName} as {_role}</p>
            <p><a href='{_acceptLink}'>Accept invitation</a></p>
            <p>If you dont want to join work in this restaurant, or got this email as mistake you can just ignore it.</p>
        ";
    }
}