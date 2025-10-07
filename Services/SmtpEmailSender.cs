using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;

namespace ClientPortal.Services;

public class SmtpEmailSender : IEmailSenderEx
{
    private readonly SmtpOptions _opts;
    public SmtpEmailSender(IOptions<SmtpOptions> opts) => _opts = opts.Value;

    public async Task SendAsync(string toEmail, string subject, string htmlBody)
    {
        using var client = new SmtpClient(_opts.Host, _opts.Port)
        {
            EnableSsl = true,
            Credentials = new NetworkCredential(_opts.User, _opts.Pass)
        };
        var msg = new MailMessage
        {
            From = new MailAddress(_opts.User, _opts.FromName),
            Subject = subject,
            Body = htmlBody,
            IsBodyHtml = true
        };
        msg.To.Add(toEmail);
        await client.SendMailAsync(msg);
    }
}
