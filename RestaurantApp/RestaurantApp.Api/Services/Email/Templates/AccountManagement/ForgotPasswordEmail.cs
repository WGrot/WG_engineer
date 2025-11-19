namespace RestaurantApp.Api.Services.Email.Templates.AccountManagement;

public class ForgotPasswordEmail: IEmailTemplate
{
    private readonly string _username;
    private string _confirmationLink;

    public ForgotPasswordEmail(string username, string confirmationLink)
    {
        _confirmationLink = confirmationLink;
        _username = username;
    }

    public string Subject => "Thank you for choosing DineOps";

    public string BuildBody()
    {
        return $@"
        <h2>Hello {_username}!</h2>
        <p>You requested to reset your password in DineOps.</p>
        <p><a href='{_confirmationLink}'>Reset Password</a></p>
        <p>This link will expire in 24 hours.</p>
        <p>If you didn't request a password reset, please ignore this message.</p>
    ";
    }
}