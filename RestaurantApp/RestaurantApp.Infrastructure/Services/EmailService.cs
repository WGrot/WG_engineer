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
        var fromName = _config["EmailSettings:FromName"];
        var fromEmail = _config["EmailSettings:FromEmail"];
        var smtpServer = _config["EmailSettings:SmtpServer"];
        var port = _config["EmailSettings:Port"];
        var username = _config["EmailSettings:Username"];
        var password = _config["EmailSettings:Password"];

        if (string.IsNullOrEmpty(fromName) ||
            string.IsNullOrEmpty(fromEmail) ||
            string.IsNullOrEmpty(smtpServer) ||
            string.IsNullOrEmpty(port) ||
            string.IsNullOrEmpty(username) ||
            string.IsNullOrEmpty(password))
        {
            return;
        }
        
        
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(fromName, fromEmail));
        message.To.Add(new MailboxAddress("", to));
        message.Subject = subject;
        message.Body = new TextPart("html") { Text = body };

        using var client = new SmtpClient();
        await client.ConnectAsync(smtpServer, int.Parse(port), false);
        await client.AuthenticateAsync(username, password);
        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }
    
}