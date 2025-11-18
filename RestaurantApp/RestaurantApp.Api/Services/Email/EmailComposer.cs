using RestaurantApp.Api.Services.Interfaces;

namespace RestaurantApp.Api.Services.Email;

public class EmailComposer : IEmailComposer
{
    private readonly IEmailService _emailService;

    public EmailComposer(IEmailService emailService)
    {
        _emailService = emailService;
    }

    public Task SendAsync(string to, IEmailTemplate template)
    {
        return _emailService.SendEmailAsync(to, template.Subject, template.BuildBody());
    }
}