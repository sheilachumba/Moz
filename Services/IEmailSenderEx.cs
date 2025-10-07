namespace ClientPortal.Services;

public interface IEmailSenderEx
{
    Task SendAsync(string toEmail, string subject, string htmlBody);
}

public class SmtpOptions
{
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 587;
    public string User { get; set; } = string.Empty;
    public string Pass { get; set; } = string.Empty;
    public string FromName { get; set; } = "Client Portal";
    public string FromEmail => User;
}
