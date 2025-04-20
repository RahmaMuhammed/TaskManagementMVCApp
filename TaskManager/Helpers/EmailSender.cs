using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using TaskManager.Helpers;

public class EmailSender : ICustomEmailSender
{
    // Hold the email settings from configuration (like SMTP server, port, credentials)
    private readonly EmailSettings _settings;

    // Constructor to inject the email settings from appsettings.json
    public EmailSender(IOptions<EmailSettings> emailSettings)
    {
        _settings = emailSettings.Value;
    }

    // Method to send an email
    public async Task SendEmailAsync(string toEmail, string subject, string htmlMessage)
    {
        try
        {
            // Create a new email message
            var message = new MimeMessage();

            // Set the sender's email address and name
            message.From.Add(new MailboxAddress(_settings.SenderName, _settings.SenderEmail));

            // Set the recipient's email address
            message.To.Add(MailboxAddress.Parse(toEmail));

            // Set the subject of the email
            message.Subject = subject;

            // Set the email body (HTML content)
            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = htmlMessage
            };
            message.Body = bodyBuilder.ToMessageBody();

            // Create the SMTP client to send the email
            using var client = new SmtpClient();

            // Connect to the SMTP server with TLS security
            await client.ConnectAsync(
                _settings.SmtpServer,
                _settings.SmtpPort,
                SecureSocketOptions.StartTlsWhenAvailable);

            // Authenticate with the SMTP server using username and password
            await client.AuthenticateAsync(
                _settings.Username,
                _settings.Password);

            // Send the email
            await client.SendAsync(message);

            // Disconnect from the server
            await client.DisconnectAsync(true);
        }
        catch (Exception ex)
        {
            // Log any error to the console and rethrow
            Console.WriteLine($"!!! EMAIL ERROR: {ex.Message}");
            throw;
        }
    }
}
