namespace RestaurantApp.Application.Services.Email.Templates.AccountManagement;

public class EmailConfirmationEmail: IEmailTemplate
{
    private readonly string _username;
    private string _confirmationLink;

    public EmailConfirmationEmail(string username, string confirmationLink)
    {
        _confirmationLink = confirmationLink;
        _username = username;
    }

    public string Subject => "Thank you for choosing DineOps";

    public string BuildBody()
    {
        return $@"
            <h2>Hello {_username}!</h2>
            <p>TO confirm your email in DineOps click this link</p>
            <p><a href='{_confirmationLink}'>Confirm email</a></p>
            <p>If you didn't sign in to our site ignore this message.</p>
        ";
    }
}