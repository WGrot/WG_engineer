namespace RestaurantApp.Api.Services.Email.Templates.AccountManagement;

public class EmployeeAccontCreated : IEmailTemplate
{
    private readonly string _username;

    private readonly string _tempPassword;

    public EmployeeAccontCreated(string username, string tempPassword)
    {
        _username = username;
        _tempPassword = tempPassword;
    }

    public string Subject => "Your employee account in DineOps";

    public string BuildBody()
    {
        return $@"
            <h2>Hello {_username}!</h2>
            <p>Your employee account has been created in DineOps</p>
            <p>Temporary Password: {_tempPassword}</p>
            <p>Change this password immediately after logging in!</p>
            <p>If you didn't sign in to our site ignore this message.</p>
        ";
    }
}