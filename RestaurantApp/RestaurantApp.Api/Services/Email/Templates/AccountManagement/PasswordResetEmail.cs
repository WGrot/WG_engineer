namespace RestaurantApp.Api.Services.Email.Templates.AccountManagement;

public class PasswordResetEmail: IEmailTemplate
{
    private readonly string _username;


    public PasswordResetEmail(string username)
    {
        _username = username;
    }

    public string Subject => "Thank you for choosing DineOps";

    public string BuildBody()
    {
        return $@"
        <h2>Hello {_username}!</h2>
        <p>Your password has been successfully changed.</p>
        <p>If you didn't make this change, please contact support immediately.</p>
    ";
    }
}