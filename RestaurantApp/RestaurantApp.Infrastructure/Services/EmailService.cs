using MailKit.Net.Smtp;

using Microsoft.Extensions.Configuration;
using MimeKit;
using RestaurantApp.Application.Interfaces;

namespace RestaurantApp.Infrastructure.Services;

public class EmailService: IEmailService
{
    private readonly IConfiguration _config;

    public EmailService(IConfiguration config)
    {
        _config = config;
    }
    
    public async Task SendEmailAsync(string to, string subject, string body)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(
            _config["EmailSettings:FromName"],
            _config["EmailSettings:FromEmail"]
        ));
        message.To.Add(new MailboxAddress("", to));
        message.Subject = subject;
        message.Body = new TextPart("html") { Text = body };

        using var client = new SmtpClient();
        await client.ConnectAsync(
            _config["EmailSettings:SmtpServer"],
            int.Parse((string)_config["EmailSettings:Port"]),
            false
        );
        await client.AuthenticateAsync(
            _config["EmailSettings:Username"],
            _config["EmailSettings:Password"]
        );
        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }
    
}