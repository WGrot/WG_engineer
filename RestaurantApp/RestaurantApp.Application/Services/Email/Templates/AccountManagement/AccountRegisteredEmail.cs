namespace RestaurantApp.Application.Services.Email.Templates.AccountManagement;

public class AccountRegisteredEmail: IEmailTemplate
{
    private readonly string _username;
    private string _confirmationLink;

    public AccountRegisteredEmail(string username, string confirmationLink)
    {
        _confirmationLink = confirmationLink;
        _username = username;
    }

    public string Subject => "Thank you for choosing DineOps";

    public string BuildBody()
    {
        return $@"
                <h2>Hello {_username}!</h2>
                <p>Thank you for registering in DineOps, Click this link to verify your email:</p>
                <p><a href='{_confirmationLink}'>Confirm email</a></p>
                <p>If you didn't sign in to our site ignore this message</p>
            ";
    }
}