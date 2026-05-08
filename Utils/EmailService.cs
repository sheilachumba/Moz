using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;

namespace MOZ_UPGRADE.Utils
{
    public interface IEmailService
    {
        Task<bool> SendCodeAsync(string email, string code, string displayName);
        Task<bool> SendVerificationEmailAsync(string email, string fullName, string verificationLink);
        Task<bool> SendQuoteSelectedNotificationAsync(string toEmail, string customerName, string quoteNo, string documentNo, string underwriter, string planArea, string module, string insuredNo, string bcLink);
        Task<bool> SendProductSubmissionNotificationAsync(IEnumerable<string> toEmails, string customerName, string contactNo, string planNo, string planDescription, DateTime startDate, DateTime endDate, string underwriter, string policyNo);
        Task<bool> SendPolicyRenewalNotificationAsync(IEnumerable<string> toEmails, string customerName, string contactNo, string oldPolicyNo, string newPlanNo, string newPlanDescription, DateTime startDate, DateTime endDate, string underwriter);
        Task<bool> SendPaymentNotificationAsync(IEnumerable<string> toEmails, string customerName, string insuredNo, string policyNo, string debitNoteNo, decimal amount, string modeOfPayment, string paymentRefNo, DateTime paymentDate);
        Task<bool> SendKycSubmissionNotificationAsync(IEnumerable<string> toEmails, string companyName, string contactNo, string nuit, string approvalStatus, DateTime submittedAt);
        Task<bool> SendClaimSubmissionNotificationAsync(IEnumerable<string> toEmails, string insuredName, string insuredNo, string policyNo, string lossType, string lossDescription, DateTime dateOfOccurrence, DateTime dateNotified);
        Task<bool> SendPaymentNotProcessedNotificationAsync(IEnumerable<string> toEmails, string insuredName, string insuredNo, string policyNo, string userDescription);
        Task<bool> SendPasswordResetEmailAsync(string toEmail, string fullName, string resetLink);
    }

    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<bool> SendCodeAsync(string email, string code, string displayName)
        {
            try
            {
                var smtpSettings = _configuration.GetSection("Smtp");
                var host = smtpSettings["Host"];
                var port = int.Parse(smtpSettings["Port"] ?? "587");
                var user = smtpSettings["User"];
                var pass = smtpSettings["Pass"];
                var from = smtpSettings["From"];
                var fromName = smtpSettings["FromName"];
                var enableSsl = bool.Parse(smtpSettings["EnableSsl"] ?? "true");

                var codeExpiryMinutes = _configuration.GetValue<int>("TwoFactorAuth:CodeExpiryMinutes", 10);

                var subject = "Your Two-Factor Authentication Code - Standard Insurance";
                var body = Generate2FAEmailHtml(displayName, code, codeExpiryMinutes);

                using (var client = new SmtpClient(host, port))
                {
                    client.EnableSsl = enableSsl;
                    client.Credentials = new NetworkCredential(user, pass);
                    client.Timeout = 10000;

                    var mailMessage = new MailMessage
                    {
                        From = new MailAddress(from, fromName),
                        Subject = subject,
                        Body = body,
                        IsBodyHtml = true
                    };

                    mailMessage.To.Add(email);

                    await client.SendMailAsync(mailMessage);
                }

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Email sending failed: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SendPasswordResetEmailAsync(string toEmail, string fullName, string resetLink)
        {
            try
            {
                var smtpSettings = _configuration.GetSection("Smtp");
                var host = smtpSettings["Host"];
                var port = int.Parse(smtpSettings["Port"] ?? "587");
                var user = smtpSettings["User"];
                var pass = smtpSettings["Pass"];
                var from = smtpSettings["From"];
                var fromName = smtpSettings["FromName"] ?? "Standard Insurance";
                var enableSsl = bool.Parse(smtpSettings["EnableSsl"] ?? "true");

                var subject = "Reset your password - Standard Insurance";

                var safeName = string.IsNullOrWhiteSpace(fullName) ? toEmail : fullName;

                var body = $@"<!DOCTYPE html>
<html lang='en'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Password Reset</title>
    <style>
        body {{ margin:0; padding:0; background:#f3f4f6; font-family:Segoe UI, Arial, sans-serif; }}
        .wrapper {{ width:100%; background:#f3f4f6; padding:24px 8px; box-sizing:border-box; }}
        .container {{ max-width:640px; margin:0 auto; background:#ffffff; border-radius:12px; box-shadow:0 18px 45px rgba(15,23,42,0.18); overflow:hidden; }}
        .header {{ background:linear-gradient(135deg,#17275F,#2A4A9F); padding:18px 24px; color:#f9fafb; }}
        .header-title {{ margin:0; font-size:18px; font-weight:700; }}
        .header-sub {{ margin:4px 0 0 0; font-size:12px; opacity:0.9; }}
        .content {{ padding:22px 24px 20px 24px; font-size:13px; color:#111827; }}
        .content p {{ margin:0 0 12px 0; line-height:1.5; }}
        .btn {{ display:inline-block; margin-top:16px; padding:10px 20px; background:#2563eb; color:#ffffff !important; text-decoration:none; border-radius:9999px; font-size:13px; font-weight:600; box-shadow:0 10px 25px rgba(37,99,235,0.35); }}
        .footer {{ margin-top:18px; font-size:11px; color:#6b7280; line-height:1.4; border-top:1px solid #e5e7eb; padding-top:12px; }}
    </style>
</head>
<body>
    <div class='wrapper'>
        <div class='container'>
            <div class='header'>
                <h1 class='header-title'>Password Reset Request</h1>
                <p class='header-sub'>You requested to reset your password for the Standard Insurance client portal.</p>
            </div>
            <div class='content'>
                <p>Dear {WebUtility.HtmlEncode(safeName)},</p>
                <p>We received a request to reset the password associated with this email address. If you made this request, please click the button below to choose a new password.</p>
                <p>
                    <a href='{WebUtility.HtmlEncode(resetLink)}' class='btn' target='_blank'>Reset Password</a>
                </p>
                <p>If the button does not work, copy and paste this link into your browser:</p>
                <p style='word-break:break-all; font-size:12px;'>{WebUtility.HtmlEncode(resetLink)}</p>
                <p>If you did not request a password reset, you can safely ignore this email and your password will remain unchanged.</p>
                <p>Kind regards,<br/>Standard Insurance Client Portal</p>
                <div class='footer'>
                    <div>Standard Insurance Corretores de Seguros, SA</div>
                    <div>Mozambique | Phone: +258-762-065-500</div>
                </div>
            </div>
        </div>
    </div>
</body>
</html>";

                using (var client = new SmtpClient(host, port))
                {
                    client.EnableSsl = enableSsl;
                    client.Credentials = new NetworkCredential(user, pass);
                    client.Timeout = 10000;

                    var mailMessage = new MailMessage
                    {
                        From = new MailAddress(from, fromName),
                        Subject = subject,
                        Body = body,
                        IsBodyHtml = true
                    };

                    mailMessage.To.Add(toEmail);

                    await client.SendMailAsync(mailMessage);
                }

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Password reset email sending failed: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SendPaymentNotProcessedNotificationAsync(IEnumerable<string> toEmails, string insuredName, string insuredNo, string policyNo, string userDescription)
        {
            try
            {
                var smtpSettings = _configuration.GetSection("Smtp");
                var host = smtpSettings["Host"];
                var port = int.Parse(smtpSettings["Port"] ?? "587");
                var user = smtpSettings["User"];
                var pass = smtpSettings["Pass"];
                var from = smtpSettings["From"];
                var fromName = smtpSettings["FromName"] ?? "Standard Insurance";
                var enableSsl = bool.Parse(smtpSettings["EnableSsl"] ?? "true");

                var subject = "Client Reports Unprocessed Payment";

                var body = $@"<!DOCTYPE html>
<html lang='en'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Unprocessed Payment Alert</title>
    <style>
        body {{ margin:0; padding:0; background:#f3f4f6; font-family:Segoe UI, Arial, sans-serif; }}
        .wrapper {{ width:100%; background:#f3f4f6; padding:24px 8px; box-sizing:border-box; }}
        .container {{ max-width:640px; margin:0 auto; background:#ffffff; border-radius:12px; box-shadow:0 18px 45px rgba(15,23,42,0.18); overflow:hidden; }}
        .header {{ background:linear-gradient(135deg,#17275F,#2A4A9F); padding:18px 24px; color:#f9fafb; }}
        .header-title {{ margin:0; font-size:18px; font-weight:700; }}
        .header-sub {{ margin:4px 0 0 0; font-size:12px; opacity:0.9; }}
        .content {{ padding:22px 24px 20px 24px; font-size:13px; color:#111827; }}
        .content p {{ margin:0 0 12px 0; line-height:1.5; }}
        .card {{ border:1px solid #e5e7eb; border-radius:10px; padding:14px 16px; margin:10px 0 16px 0; background:#f9fafb; }}
        .card-title {{ font-size:13px; font-weight:600; margin:0 0 8px 0; color:#111827; }}
        .details-table {{ border-collapse:collapse; width:100%; font-size:12px; }}
        .details-table td {{ padding:4px 0; vertical-align:top; }}
        .label {{ width:150px; font-weight:600; color:#4b5563; }}
        .value {{ color:#111827; }}
        .footer {{ margin-top:18px; font-size:11px; color:#6b7280; line-height:1.4; border-top:1px solid #e5e7eb; padding-top:12px; }}
        .footer-links a {{ color:#2563eb; text-decoration:none; margin-right:8px; }}
        .footer-links a:last-child {{ margin-right:0; }}
    </style>
</head>
<body>
    <div class='wrapper'>
        <div class='container'>
            <div class='header'>
                <h1 class='header-title'>Client Reports Unprocessed Payment</h1>
                <p class='header-sub'>A client indicates they have made a payment which is not yet reflected in ERP.</p>
            </div>
            <div class='content'>
                <p>Dear Team,</p>
                <p>The client has reported that they have already made a payment for their policy, but it is not yet visible in the portal/ERP. Please review and reconcile as appropriate.</p>

                <div class='card'>
                    <p class='card-title'>Client & Policy Details</p>
                    <table class='details-table'>
                        <tr><td class='label'>Insured Name</td><td class='value'>{WebUtility.HtmlEncode(insuredName ?? string.Empty)}</td></tr>
                        <tr><td class='label'>Insured No</td><td class='value'>{WebUtility.HtmlEncode(insuredNo ?? string.Empty)}</td></tr>
                        <tr><td class='label'>Policy No</td><td class='value'>{WebUtility.HtmlEncode(policyNo ?? string.Empty)}</td></tr>
                    </table>
                </div>

                <div class='card'>
                    <p class='card-title'>Client's Message</p>
                    <p>{WebUtility.HtmlEncode(userDescription ?? string.Empty)}</p>
                </div>

                <p>Kind regards,<br/>Standard Insurance Client Portal</p>

                <div class='footer'>
                    <div>Standard Insurance Corretores de Seguros, SA</div>
                    <div>Mozambique | Phone: +258-762-065-500</div>
                    <div class='footer-links' style='margin-top:6px;'>
                        <a href='#'>Privacy Policy</a>|
                        <a href='#'>Terms of Service</a>|
                        <a href='#'>Contact Us</a>
                    </div>
                    <div style='margin-top:4px;'>&copy; 2025 Standard Insurance. All rights reserved.</div>
                </div>
            </div>
        </div>
    </div>
</body>
</html>";

                using (var client = new SmtpClient(host, port))
                {
                    client.EnableSsl = enableSsl;
                    client.Credentials = new NetworkCredential(user, pass);
                    client.Timeout = 10000;

                    var mailMessage = new MailMessage
                    {
                        From = new MailAddress(from, fromName),
                        Subject = subject,
                        Body = body,
                        IsBodyHtml = true
                    };

                    foreach (var addr in toEmails ?? Array.Empty<string>())
                    {
                        if (!string.IsNullOrWhiteSpace(addr))
                            mailMessage.To.Add(addr.Trim());
                    }

                    if (mailMessage.To.Count == 0)
                        return false;

                    await client.SendMailAsync(mailMessage);
                }

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Payment not processed email sending failed: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SendPaymentNotificationAsync(IEnumerable<string> toEmails, string customerName, string insuredNo, string policyNo, string debitNoteNo, decimal amount, string modeOfPayment, string paymentRefNo, DateTime paymentDate)
        {
            try
            {
                var smtpSettings = _configuration.GetSection("Smtp");
                var host = smtpSettings["Host"];
                var port = int.Parse(smtpSettings["Port"] ?? "587");
                var user = smtpSettings["User"];
                var pass = smtpSettings["Pass"];
                var from = smtpSettings["From"];
                var fromName = smtpSettings["FromName"] ?? "Standard Insurance";
                var enableSsl = bool.Parse(smtpSettings["EnableSsl"] ?? "true");

                var subject = "Client Payment Notification";
                var formattedAmount = amount.ToString("N2");

                var body = $@"<!DOCTYPE html>
<html lang='en'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Client Payment Notification</title>
    <style>
        body {{ margin:0; padding:0; background:#f3f4f6; font-family:Segoe UI, Arial, sans-serif; }}
        .wrapper {{ width:100%; background:#f3f4f6; padding:24px 8px; box-sizing:border-box; }}
        .container {{ max-width:640px; margin:0 auto; background:#ffffff; border-radius:12px; box-shadow:0 18px 45px rgba(15,23,42,0.18); overflow:hidden; }}
        .header {{ background:linear-gradient(135deg,#17275F,#2A4A9F); padding:18px 24px; color:#f9fafb; }}
        .header-title {{ margin:0; font-size:18px; font-weight:700; }}
        .header-sub {{ margin:4px 0 0 0; font-size:12px; opacity:0.9; }}
        .content {{ padding:22px 24px 20px 24px; font-size:13px; color:#111827; }}
        .content p {{ margin:0 0 12px 0; line-height:1.5; }}
        .card {{ border:1px solid #e5e7eb; border-radius:10px; padding:14px 16px; margin:10px 0 16px 0; background:#f9fafb; }}
        .card-title {{ font-size:13px; font-weight:600; margin:0 0 8px 0; color:#111827; }}
        .details-table {{ border-collapse:collapse; width:100%; font-size:12px; }}
        .details-table td {{ padding:4px 0; vertical-align:top; }}
        .label {{ width:150px; font-weight:600; color:#4b5563; }}
        .value {{ color:#111827; }}
        .amount-pill {{ display:inline-block; padding:4px 10px; border-radius:999px; background:#ecfdf3; color:#15803d; font-weight:700; font-size:12px; }}
        .footer {{ margin-top:18px; font-size:11px; color:#6b7280; line-height:1.4; border-top:1px solid #e5e7eb; padding-top:12px; }}
        .footer-links a {{ color:#2563eb; text-decoration:none; margin-right:8px; }}
        .footer-links a:last-child {{ margin-right:0; }}
    </style>
</head>
<body>
    <div class='wrapper'>
        <div class='container'>
            <div class='header'>
                <h1 class='header-title'>Client Payment Received</h1>
                <p class='header-sub'>A customer has made a payment in the client portal.</p>
            </div>
            <div class='content'>
                <p>Dear Team,</p>
                <p>The following payment has just been submitted from the client portal:</p>

                <div class='card'>
                    <p class='card-title'>Payment Summary</p>
                    <table class='details-table'>
                        <tr><td class='label'>Client Name</td><td class='value'>{WebUtility.HtmlEncode(customerName ?? string.Empty)}</td></tr>
                        <tr><td class='label'>Insured No</td><td class='value'>{WebUtility.HtmlEncode(insuredNo ?? string.Empty)}</td></tr>
                        <tr><td class='label'>Policy No</td><td class='value'>{WebUtility.HtmlEncode(policyNo ?? string.Empty)}</td></tr>
                        <tr><td class='label'>Debit Note No</td><td class='value'>{WebUtility.HtmlEncode(debitNoteNo ?? string.Empty)}</td></tr>
                        <tr><td class='label'>Amount</td><td class='value'><span class='amount-pill'>{formattedAmount}</span></td></tr>
                        <tr><td class='label'>Mode of Payment</td><td class='value'>{WebUtility.HtmlEncode(modeOfPayment ?? string.Empty)}</td></tr>
                        <tr><td class='label'>Payment Ref No</td><td class='value'>{WebUtility.HtmlEncode(paymentRefNo ?? string.Empty)}</td></tr>
                        <tr><td class='label'>Payment Date</td><td class='value'>{paymentDate:yyyy-MM-dd}</td></tr>
                    </table>
                </div>

                <p>This notification is for tracking purposes only. Please reconcile this payment in ERP as per your normal process.</p>

                <p>Kind regards,<br/>Standard Insurance Client Portal</p>

                <div class='footer'>
                    <div>Standard Insurance Corretores de Seguros, SA</div>
                    <div>Mozambique | Phone: +258-762-065-500</div>
                    <div class='footer-links' style='margin-top:6px;'>
                        <a href='#'>Privacy Policy</a>|
                        <a href='#'>Terms of Service</a>|
                        <a href='#'>Contact Us</a>
                    </div>
                    <div style='margin-top:4px;'>&copy; 2025 Standard Insurance. All rights reserved.</div>
                </div>
            </div>
        </div>
    </div>
</body>
</html>";

                using (var client = new SmtpClient(host, port))
                {
                    client.EnableSsl = enableSsl;
                    client.Credentials = new NetworkCredential(user, pass);
                    client.Timeout = 10000;

                    var mailMessage = new MailMessage
                    {
                        From = new MailAddress(from, fromName),
                        Subject = subject,
                        Body = body,
                        IsBodyHtml = true
                    };

                    foreach (var addr in toEmails ?? Array.Empty<string>())
                    {
                        if (!string.IsNullOrWhiteSpace(addr))
                            mailMessage.To.Add(addr.Trim());
                    }

                    if (mailMessage.To.Count == 0)
                        return false;

                    await client.SendMailAsync(mailMessage);
                }

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Payment notification email sending failed: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SendKycSubmissionNotificationAsync(IEnumerable<string> toEmails, string companyName, string contactNo, string nuit, string approvalStatus, DateTime submittedAt)
        {
            try
            {
                var smtpSettings = _configuration.GetSection("Smtp");
                var host = smtpSettings["Host"];
                var port = int.Parse(smtpSettings["Port"] ?? "587");
                var user = smtpSettings["User"];
                var pass = smtpSettings["Pass"];
                var from = smtpSettings["From"];
                var fromName = smtpSettings["FromName"] ?? "Standard Insurance";
                var enableSsl = bool.Parse(smtpSettings["EnableSsl"] ?? "true");

                var bcBaseUrl = _configuration["BcApi:BaseUrl"];
                var bcLink = $"{bcBaseUrl}Company('STANDARD%20INSURANCE')/ContactCard";

                var subject = "New KYC Application Submitted";

                var body = $@"<!DOCTYPE html>
<html lang='en'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>New KYC Application Submitted</title>
    <style>
        body {{ margin:0; padding:0; background:#f3f4f6; font-family:Segoe UI, Arial, sans-serif; }}
        .wrapper {{ width:100%; background:#f3f4f6; padding:24px 8px; box-sizing:border-box; }}
        .container {{ max-width:640px; margin:0 auto; background:#ffffff; border-radius:12px; box-shadow:0 18px 45px rgba(15,23,42,0.18); overflow:hidden; }}
        .header {{ background:linear-gradient(135deg,#17275F,#2A4A9F); padding:18px 24px; color:#f9fafb; }}
        .header-title {{ margin:0; font-size:18px; font-weight:700; }}
        .header-sub {{ margin:4px 0 0 0; font-size:12px; opacity:0.9; }}
        .content {{ padding:22px 24px 20px 24px; font-size:13px; color:#111827; }}
        .content p {{ margin:0 0 12px 0; line-height:1.5; }}
        .card {{ border:1px solid #e5e7eb; border-radius:10px; padding:14px 16px; margin:10px 0 16px 0; background:#f9fafb; }}
        .card-title {{ font-size:13px; font-weight:600; margin:0 0 8px 0; color:#111827; }}
        .details-table {{ border-collapse:collapse; width:100%; font-size:12px; }}
        .details-table td {{ padding:4px 0; vertical-align:top; }}
        .label {{ width:150px; font-weight:600; color:#4b5563; }}
        .value {{ color:#111827; }}
        .link-btn {{ display:inline-block; margin-top:14px; padding:9px 18px; background:#2563eb; color:#ffffff !important; text-decoration:none; border-radius:9999px; font-size:12px; font-weight:600; box-shadow:0 10px 25px rgba(37,99,235,0.35); }}
        .footer {{ margin-top:18px; font-size:11px; color:#6b7280; line-height:1.4; border-top:1px solid #e5e7eb; padding-top:12px; }}
        .footer-links a {{ color:#2563eb; text-decoration:none; margin-right:8px; }}
        .footer-links a:last-child {{ margin-right:0; }}
    </style>
</head>
<body>
    <div class='wrapper'>
        <div class='container'>
            <div class='header'>
                <h1 class='header-title'>New KYC Application</h1>
                <p class='header-sub'>A corporate client has submitted a new KYC application.</p>
            </div>
            <div class='content'>
                <p>Dear Team,</p>
                <p>The following KYC application has just been submitted from the client portal:</p>

                <div class='card'>
                    <p class='card-title'>KYC Summary</p>
                    <table class='details-table'>
                        <tr><td class='label'>Company Name</td><td class='value'>{WebUtility.HtmlEncode(companyName ?? string.Empty)}</td></tr>
                        <tr><td class='label'>Contact No</td><td class='value'>{WebUtility.HtmlEncode(contactNo ?? string.Empty)}</td></tr>
                        <tr><td class='label'>NUIT / BVN_No</td><td class='value'>{WebUtility.HtmlEncode(nuit ?? string.Empty)}</td></tr>
                        <tr><td class='label'>Approval Status</td><td class='value'>{WebUtility.HtmlEncode(approvalStatus ?? string.Empty)}</td></tr>
                        <tr><td class='label'>Submitted At</td><td class='value'>{submittedAt:yyyy-MM-dd HH:mm}</td></tr>
                    </table>
                </div>

                <p>You can review this KYC in ERP using the link below:</p>
                <p>
                    <a href='{WebUtility.HtmlEncode(bcLink)}' target='_blank' class='link-btn'>Open Contact Cards in ERP</a>
                </p>

                <p>Kind regards,<br/>Standard Insurance Client Portal</p>

                <div class='footer'>
                    <div>Standard Insurance Corretores de Seguros, SA</div>
                    <div>Mozambique | Phone: +258-762-065-500</div>
                    <div class='footer-links' style='margin-top:6px;'>
                        <a href='#'>Privacy Policy</a>|
                        <a href='#'>Terms of Service</a>|
                        <a href='#'>Contact Us</a>
                    </div>
                    <div style='margin-top:4px;'>&copy; 2025 Standard Insurance. All rights reserved.</div>
                </div>
            </div>
        </div>
    </div>
</body>
</html>";

                using (var client = new SmtpClient(host, port))
                {
                    client.EnableSsl = enableSsl;
                    client.Credentials = new NetworkCredential(user, pass);
                    client.Timeout = 10000;

                    var mailMessage = new MailMessage
                    {
                        From = new MailAddress(from, fromName),
                        Subject = subject,
                        Body = body,
                        IsBodyHtml = true
                    };

                    foreach (var addr in toEmails ?? Array.Empty<string>())
                    {
                        if (!string.IsNullOrWhiteSpace(addr))
                            mailMessage.To.Add(addr.Trim());
                    }

                    if (mailMessage.To.Count == 0)
                        return false;

                    await client.SendMailAsync(mailMessage);
                }

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"KYC submission email sending failed: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SendProductSubmissionNotificationAsync(IEnumerable<string> toEmails, string customerName, string contactNo, string planNo, string planDescription, DateTime startDate, DateTime endDate, string underwriter, string policyNo)
        {
            try
            {
                var smtpSettings = _configuration.GetSection("Smtp");
                var host = smtpSettings["Host"];
                var port = int.Parse(smtpSettings["Port"] ?? "587");
                var user = smtpSettings["User"];
                var pass = smtpSettings["Pass"];
                var from = smtpSettings["From"];
                var fromName = smtpSettings["FromName"] ?? "Standard Insurance";
                var enableSsl = bool.Parse(smtpSettings["EnableSsl"] ?? "true");

                // Default BC link to ProductSelection list as per user requirement
                var bcBaseUrl = _configuration["BcApi:BaseUrl"];
                var bcLink = $"{bcBaseUrl}Company('STANDARD%20INSURANCE')/ProductSelection";

                var subject = "New Product Submitted for Quote Review";
                var body = $@"<!DOCTYPE html>
<html lang='en'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Product Submitted for Quote Review</title>
    <style>
        body {{ margin:0; padding:0; background:#f3f4f6; font-family:Segoe UI, Arial, sans-serif; }}
        .wrapper {{ width:100%; background:#f3f4f6; padding:24px 8px; box-sizing:border-box; }}
        .container {{ max-width:640px; margin:0 auto; background:#ffffff; border-radius:12px; box-shadow:0 18px 45px rgba(15,23,42,0.18); overflow:hidden; }}
        .header {{ background:linear-gradient(135deg,#17275F,#2A4A9F); padding:18px 24px; color:#f9fafb; }}
        .header-title {{ margin:0; font-size:18px; font-weight:700; }}
        .header-sub {{ margin:4px 0 0 0; font-size:12px; opacity:0.9; }}
        .content {{ padding:22px 24px 20px 24px; font-size:13px; color:#111827; }}
        .content p {{ margin:0 0 12px 0; line-height:1.5; }}
        .card {{ border:1px solid #e5e7eb; border-radius:10px; padding:14px 16px; margin:10px 0 16px 0; background:#f9fafb; }}
        .card-title {{ font-size:13px; font-weight:600; margin:0 0 8px 0; color:#111827; }}
        .details-table {{ border-collapse:collapse; width:100%; font-size:12px; }}
        .details-table td {{ padding:4px 0; vertical-align:top; }}
        .label {{ width:150px; font-weight:600; color:#4b5563; }}
        .value {{ color:#111827; }}
        .link-btn {{ display:inline-block; margin-top:14px; padding:9px 18px; background:#2563eb; color:#ffffff !important; text-decoration:none; border-radius:9999px; font-size:12px; font-weight:600; box-shadow:0 10px 25px rgba(37,99,235,0.35); }}
        .footer {{ margin-top:18px; font-size:11px; color:#6b7280; line-height:1.4; border-top:1px solid #e5e7eb; padding-top:12px; }}
        .footer-links a {{ color:#2563eb; text-decoration:none; margin-right:8px; }}
        .footer-links a:last-child {{ margin-right:0; }}
    </style>
</head>
<body>
    <div class='wrapper'>
        <div class='container'>
            <div class='header'>
                <h1 class='header-title'>New Product Submitted</h1>
                <p class='header-sub'>A client has submitted a product for quote review.</p>
            </div>
            <div class='content'>
                <p>Dear Team,</p>
                <p>The following product has just been submitted from the client portal for quote review:</p>

                <div class='card'>
                    <p class='card-title'>Submission Summary</p>
                    <table class='details-table'>
                        <tr><td class='label'>Client Name</td><td class='value'>{WebUtility.HtmlEncode(customerName)}</td></tr>
                        <tr><td class='label'>Contact No</td><td class='value'>{WebUtility.HtmlEncode(contactNo ?? string.Empty)}</td></tr>
                        <tr><td class='label'>Policy No</td><td class='value'>{WebUtility.HtmlEncode(policyNo ?? string.Empty)}</td></tr>
                        <tr><td class='label'>Plan</td><td class='value'>{WebUtility.HtmlEncode(planNo ?? string.Empty)} - {WebUtility.HtmlEncode(planDescription ?? string.Empty)}</td></tr>
                        <tr><td class='label'>Underwriter</td><td class='value'>{WebUtility.HtmlEncode(underwriter ?? string.Empty)}</td></tr>
                        <tr><td class='label'>Cover Period</td><td class='value'>{startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}</td></tr>
                    </table>
                </div>

                <p>You can review this submission and the related entries in ERP using the link below:</p>
                <p>
                    <a href='{WebUtility.HtmlEncode(bcLink)}' target='_blank' class='link-btn'>Open Product Selection in ERP</a>
                </p>

                <p>Kind regards,<br/>Standard Insurance Client Portal</p>

                <div class='footer'>
                    <div>Standard Insurance Corretores de Seguros, SA</div>
                    <div>Mozambique | Phone: +258-762-065-500</div>
                    <div class='footer-links' style='margin-top:6px;'>
                        <a href='#'>Privacy Policy</a>|
                        <a href='#'>Terms of Service</a>|
                        <a href='#'>Contact Us</a>
                    </div>
                    <div style='margin-top:4px;'>&copy; 2025 Standard Insurance. All rights reserved.</div>
                </div>
            </div>
        </div>
    </div>
</body>
</html>";

                using (var client = new SmtpClient(host, port))
                {
                    client.EnableSsl = enableSsl;
                    client.Credentials = new NetworkCredential(user, pass);
                    client.Timeout = 10000;

                    var mailMessage = new MailMessage
                    {
                        From = new MailAddress(from, fromName),
                        Subject = subject,
                        Body = body,
                        IsBodyHtml = true
                    };

                    foreach (var addr in toEmails ?? Array.Empty<string>())
                    {
                        if (!string.IsNullOrWhiteSpace(addr))
                            mailMessage.To.Add(addr.Trim());
                    }

                    if (mailMessage.To.Count == 0)
                        return false;

                    await client.SendMailAsync(mailMessage);
                }

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Product submission email sending failed: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SendPolicyRenewalNotificationAsync(IEnumerable<string> toEmails, string customerName, string contactNo, string oldPolicyNo, string newPlanNo, string newPlanDescription, DateTime startDate, DateTime endDate, string underwriter)
        {
            try
            {
                var smtpSettings = _configuration.GetSection("Smtp");
                var host = smtpSettings["Host"];
                var port = int.Parse(smtpSettings["Port"] ?? "587");
                var user = smtpSettings["User"];
                var pass = smtpSettings["Pass"];
                var from = smtpSettings["From"];
                var fromName = smtpSettings["FromName"] ?? "Standard Insurance";
                var enableSsl = bool.Parse(smtpSettings["EnableSsl"] ?? "true");

                var subject = "Policy Renewal Submitted (New Policy Selected)";

                var body = $@"<!DOCTYPE html>
<html lang='en'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Policy Renewal Submitted</title>
    <style>
        body {{ margin:0; padding:0; background:#f3f4f6; font-family:Segoe UI, Arial, sans-serif; }}
        .wrapper {{ width:100%; background:#f3f4f6; padding:24px 8px; box-sizing:border-box; }}
        .container {{ max-width:640px; margin:0 auto; background:#ffffff; border-radius:12px; box-shadow:0 18px 45px rgba(15,23,42,0.18); overflow:hidden; }}
        .header {{ background:linear-gradient(135deg,#17275F,#2A4A9F); padding:18px 24px; color:#f9fafb; }}
        .header-title {{ margin:0; font-size:18px; font-weight:700; }}
        .header-sub {{ margin:4px 0 0 0; font-size:12px; opacity:0.9; }}
        .content {{ padding:22px 24px 20px 24px; font-size:13px; color:#111827; }}
        .content p {{ margin:0 0 12px 0; line-height:1.5; }}
        .card {{ border:1px solid #e5e7eb; border-radius:10px; padding:14px 16px; margin:10px 0 16px 0; background:#f9fafb; }}
        .card-title {{ font-size:13px; font-weight:600; margin:0 0 8px 0; color:#111827; }}
        .details-table {{ border-collapse:collapse; width:100%; font-size:12px; }}
        .details-table td {{ padding:4px 0; vertical-align:top; }}
        .label {{ width:170px; font-weight:600; color:#4b5563; }}
        .value {{ color:#111827; }}
        .footer {{ margin-top:18px; font-size:11px; color:#6b7280; line-height:1.4; border-top:1px solid #e5e7eb; padding-top:12px; }}
    </style>
</head>
<body>
    <div class='wrapper'>
        <div class='container'>
            <div class='header'>
                <h1 class='header-title'>Policy Renewal Submitted</h1>
                <p class='header-sub'>A customer has submitted a renewal by selecting a new policy.</p>
            </div>
            <div class='content'>
                <p>Dear Team,</p>
                <p>A customer has submitted a policy renewal request by selecting a new policy/product in the portal. Please review and process in ERP.</p>

                <div class='card'>
                    <p class='card-title'>Customer & Renewal Details</p>
                    <table class='details-table'>
                        <tr><td class='label'>Customer Name</td><td class='value'>{WebUtility.HtmlEncode(customerName ?? string.Empty)}</td></tr>
                        <tr><td class='label'>Contact No</td><td class='value'>{WebUtility.HtmlEncode(contactNo ?? string.Empty)}</td></tr>
                        <tr><td class='label'>Old Policy No</td><td class='value'>{WebUtility.HtmlEncode(oldPolicyNo ?? string.Empty)}</td></tr>
                        <tr><td class='label'>Selected Plan</td><td class='value'>{WebUtility.HtmlEncode(newPlanNo ?? string.Empty)} - {WebUtility.HtmlEncode(newPlanDescription ?? string.Empty)}</td></tr>
                        <tr><td class='label'>Underwriter</td><td class='value'>{WebUtility.HtmlEncode(underwriter ?? string.Empty)}</td></tr>
                        <tr><td class='label'>Requested Cover Period</td><td class='value'>{startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}</td></tr>
                    </table>
                </div>

                <p>Kind regards,<br/>Standard Insurance Client Portal</p>

                <div class='footer'>
                    <div>Standard Insurance Corretores de Seguros, SA</div>
                    <div>Mozambique | Phone: +258-762-065-500</div>
                </div>
            </div>
        </div>
    </div>
</body>
</html>";

                using (var client = new SmtpClient(host, port))
                {
                    client.EnableSsl = enableSsl;
                    client.Credentials = new NetworkCredential(user, pass);
                    client.Timeout = 10000;

                    var mailMessage = new MailMessage
                    {
                        From = new MailAddress(from, fromName),
                        Subject = subject,
                        Body = body,
                        IsBodyHtml = true
                    };

                    foreach (var addr in toEmails ?? Array.Empty<string>())
                    {
                        if (!string.IsNullOrWhiteSpace(addr))
                            mailMessage.To.Add(addr.Trim());
                    }

                    if (mailMessage.To.Count == 0)
                        return false;

                    await client.SendMailAsync(mailMessage);
                }

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Policy renewal email sending failed: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SendClaimSubmissionNotificationAsync(IEnumerable<string> toEmails, string insuredName, string insuredNo, string policyNo, string lossType, string lossDescription, DateTime dateOfOccurrence, DateTime dateNotified)
        {
            try
            {
                var smtpSettings = _configuration.GetSection("Smtp");
                var host = smtpSettings["Host"];
                var port = int.Parse(smtpSettings["Port"] ?? "587");
                var user = smtpSettings["User"];
                var pass = smtpSettings["Pass"];
                var from = smtpSettings["From"];
                var fromName = smtpSettings["FromName"] ?? "Standard Insurance";
                var enableSsl = bool.Parse(smtpSettings["EnableSsl"] ?? "true");

                var subject = "New Claim Submitted";

                var body = $@"<!DOCTYPE html>
<html lang='en'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>New Claim Submitted</title>
    <style>
        body {{ margin:0; padding:0; background:#f3f4f6; font-family:Segoe UI, Arial, sans-serif; }}
        .wrapper {{ width:100%; background:#f3f4f6; padding:24px 8px; box-sizing:border-box; }}
        .container {{ max-width:640px; margin:0 auto; background:#ffffff; border-radius:12px; box-shadow:0 18px 45px rgba(15,23,42,0.18); overflow:hidden; }}
        .header {{ background:linear-gradient(135deg,#17275F,#2A4A9F); padding:18px 24px; color:#f9fafb; }}
        .header-title {{ margin:0; font-size:18px; font-weight:700; }}
        .header-sub {{ margin:4px 0 0 0; font-size:12px; opacity:0.9; }}
        .content {{ padding:22px 24px 20px 24px; font-size:13px; color:#111827; }}
        .content p {{ margin:0 0 12px 0; line-height:1.5; }}
        .card {{ border:1px solid #e5e7eb; border-radius:10px; padding:14px 16px; margin:10px 0 16px 0; background:#f9fafb; }}
        .card-title {{ font-size:13px; font-weight:600; margin:0 0 8px 0; color:#111827; }}
        .details-table {{ border-collapse:collapse; width:100%; font-size:12px; }}
        .details-table td {{ padding:4px 0; vertical-align:top; }}
        .label {{ width:150px; font-weight:600; color:#4b5563; }}
        .value {{ color:#111827; }}
        .footer {{ margin-top:18px; font-size:11px; color:#6b7280; line-height:1.4; border-top:1px solid #e5e7eb; padding-top:12px; }}
        .footer-links a {{ color:#2563eb; text-decoration:none; margin-right:8px; }}
        .footer-links a:last-child {{ margin-right:0; }}
    </style>
</head>
<body>
    <div class='wrapper'>
        <div class='container'>
            <div class='header'>
                <h1 class='header-title'>New Claim Submitted</h1>
                <p class='header-sub'>A client has submitted a claim from the client portal.</p>
            </div>
            <div class='content'>
                <p>Dear Team,</p>
                <p>The following claim has just been submitted from the client portal:</p>

                <div class='card'>
                    <p class='card-title'>Claim Summary</p>
                    <table class='details-table'>
                        <tr><td class='label'>Insured Name</td><td class='value'>{WebUtility.HtmlEncode(insuredName ?? string.Empty)}</td></tr>
                        <tr><td class='label'>Insured No</td><td class='value'>{WebUtility.HtmlEncode(insuredNo ?? string.Empty)}</td></tr>
                        <tr><td class='label'>Policy No</td><td class='value'>{WebUtility.HtmlEncode(policyNo ?? string.Empty)}</td></tr>
                        <tr><td class='label'>Loss Type</td><td class='value'>{WebUtility.HtmlEncode(lossType ?? string.Empty)}</td></tr>
                        <tr><td class='label'>Loss Description</td><td class='value'>{WebUtility.HtmlEncode(lossDescription ?? string.Empty)}</td></tr>
                        <tr><td class='label'>Date of Occurrence</td><td class='value'>{dateOfOccurrence:yyyy-MM-dd}</td></tr>
                        <tr><td class='label'>Date Notified</td><td class='value'>{dateNotified:yyyy-MM-dd}</td></tr>
                    </table>
                </div>

                <p>Kind regards,<br/>Standard Insurance Client Portal</p>

                <div class='footer'>
                    <div>Standard Insurance Corretores de Seguros, SA</div>
                    <div>Mozambique | Phone: +258-762-065-500</div>
                    <div class='footer-links' style='margin-top:6px;'>
                        <a href='#'>Privacy Policy</a>|
                        <a href='#'>Terms of Service</a>|
                        <a href='#'>Contact Us</a>
                    </div>
                    <div style='margin-top:4px;'>&copy; 2025 Standard Insurance. All rights reserved.</div>
                </div>
            </div>
        </div>
    </div>
</body>
</html>";

                using (var client = new SmtpClient(host, port))
                {
                    client.EnableSsl = enableSsl;
                    client.Credentials = new NetworkCredential(user, pass);
                    client.Timeout = 10000;

                    var mailMessage = new MailMessage
                    {
                        From = new MailAddress(from, fromName),
                        Subject = subject,
                        Body = body,
                        IsBodyHtml = true
                    };

                    foreach (var addr in toEmails ?? Array.Empty<string>())
                    {
                        if (!string.IsNullOrWhiteSpace(addr))
                            mailMessage.To.Add(addr.Trim());
                    }

                    if (mailMessage.To.Count == 0)
                        return false;

                    await client.SendMailAsync(mailMessage);
                }

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Claim submission email sending failed: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SendVerificationEmailAsync(string email, string fullName, string verificationLink)
        {
            try
            {
                var smtpSettings = _configuration.GetSection("Smtp");
                var host = smtpSettings["Host"];
                var port = int.Parse(smtpSettings["Port"] ?? "587");
                var user = smtpSettings["User"];
                var pass = smtpSettings["Pass"];
                var from = smtpSettings["From"];
                var fromName = smtpSettings["FromName"] ?? "Standard Insurance";
                var enableSsl = bool.Parse(smtpSettings["EnableSsl"] ?? "true");

                var subject = "Activate Your Standard Insurance Account";
                var body = GenerateVerificationEmailHtml(fullName, verificationLink);

                using (var client = new SmtpClient(host, port))
                {
                    client.EnableSsl = enableSsl;
                    client.Credentials = new NetworkCredential(user, pass);
                    client.Timeout = 10000;

                    var mailMessage = new MailMessage
                    {
                        From = new MailAddress(from, fromName),
                        Subject = subject,
                        Body = body,
                        IsBodyHtml = true
                    };

                    mailMessage.To.Add(email);

                    await client.SendMailAsync(mailMessage);
                }

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Verification email sending failed: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SendQuoteSelectedNotificationAsync(string toEmail, string customerName, string quoteNo, string documentNo, string underwriter, string planArea, string module, string insuredNo, string bcLink)
        {
            try
            {
                var smtpSettings = _configuration.GetSection("Smtp");
                var host = smtpSettings["Host"];
                var port = int.Parse(smtpSettings["Port"] ?? "587");
                var user = smtpSettings["User"];
                var pass = smtpSettings["Pass"];
                var from = smtpSettings["From"];
                var fromName = smtpSettings["FromName"] ?? "Standard Insurance";
                var enableSsl = bool.Parse(smtpSettings["EnableSsl"] ?? "true");

                var subject = "Customer Quote Selection Notification";
                var body = $@"<!DOCTYPE html>
<html lang='en'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Quote Selection Notification</title>
    <style>
        body {{ margin:0; padding:0; background:#f4f5f7; font-family:Segoe UI, Arial, sans-serif; }}
        .wrapper {{ width:100%; background:#f4f5f7; padding:24px 8px; box-sizing:border-box; }}
        .container {{ max-width:640px; margin:0 auto; background:#ffffff; border-radius:8px; box-shadow:0 2px 8px rgba(15,23,42,0.08); overflow:hidden; }}
        .header {{ background:#1f2937; padding:16px 24px; color:#f9fafb; }}
        .header-title {{ margin:0; font-size:18px; font-weight:600; }}
        .content {{ padding:24px; font-size:13px; color:#111827; }}
        .content p {{ margin:0 0 12px 0; line-height:1.5; }}
        .card {{ border:1px solid #e5e7eb; border-radius:8px; padding:16px 18px; margin:12px 0 18px 0; background:#f9fafb; }}
        .card-title {{ font-size:13px; font-weight:600; margin:0 0 10px 0; color:#111827; }}
        .details-table {{ border-collapse:collapse; width:100%; font-size:12px; }}
        .details-table td {{ padding:4px 0; vertical-align:top; }}
        .label {{ width:140px; font-weight:600; color:#4b5563; }}
        .value {{ color:#111827; }}
        .link-btn {{ display:inline-block; margin-top:10px; padding:8px 14px; background:#2563eb; color:#ffffff !important; text-decoration:none; border-radius:4px; font-size:12px; font-weight:600; }}
        .footer {{ margin-top:18px; font-size:11px; color:#6b7280; line-height:1.4; border-top:1px solid #e5e7eb; padding-top:12px; }}
        .footer-links a {{ color:#2563eb; text-decoration:none; margin-right:8px; }}
        .footer-links a:last-child {{ margin-right:0; }}
    </style>
    </head>
<body>
    <div class='wrapper'>
        <div class='container'>
            <div class='header'>
                <h1 class='header-title'>Quote Selection Notification</h1>
            </div>
            <div class='content'>
                <p>Dear Team,</p>
                <p>A customer has selected the following quote in the client portal:</p>

                <div class='card'>
                    <p class='card-title'>Selected Quote Details</p>
                    <table class='details-table'>
                        <tr><td class='label'>Customer</td><td class='value'>{WebUtility.HtmlEncode(customerName)}</td></tr>
                        <tr><td class='label'>Entry / Quote No</td><td class='value'>{WebUtility.HtmlEncode(quoteNo)}</td></tr>
                        <tr><td class='label'>Document No</td><td class='value'>{WebUtility.HtmlEncode(documentNo)}</td></tr>
                        <tr><td class='label'>Underwriter</td><td class='value'>{WebUtility.HtmlEncode(underwriter)}</td></tr>
                        <tr><td class='label'>Plan / Area</td><td class='value'>{WebUtility.HtmlEncode(planArea)}</td></tr>
                        <tr><td class='label'>Module</td><td class='value'>{WebUtility.HtmlEncode(module)}</td></tr>
                        <tr><td class='label'>Insured No</td><td class='value'>{WebUtility.HtmlEncode(insuredNo)}</td></tr>
                    </table>
                </div>

                <p>You can review this selection in ERP using the link below:</p>
                <p>
                    <a href='{WebUtility.HtmlEncode(bcLink)}' target='_blank' class='link-btn'>Open in ERP</a>
                </p>

                <p>Kind regards,<br/>Standard Insurance Client Portal</p>

                <div class='footer'>
                    <div>Standard Insurance Corretores de Seguros, SA</div>
                    <div>Mozambique | Phone: +258-21-50-1020 | Email: info@standardinsurance.co.mz</div>
                    <div class='footer-links' style='margin-top:6px;'>
                        <a href='#'>Privacy Policy</a>|
                        <a href='#'>Terms of Service</a>|
                        <a href='#'>Contact Us</a>
                    </div>
                    <div style='margin-top:4px;'> 2025 Standard Insurance. All rights reserved.</div>
                </div>
            </div>
        </div>
    </div>
</body>
</html>";

                using (var client = new SmtpClient(host, port))
                {
                    client.EnableSsl = enableSsl;
                    client.Credentials = new NetworkCredential(user, pass);
                    client.Timeout = 10000;

                    var mailMessage = new MailMessage
                    {
                        From = new MailAddress(from, fromName),
                        Subject = subject,
                        Body = body,
                        IsBodyHtml = true
                    };

                    mailMessage.To.Add(toEmail);

                    await client.SendMailAsync(mailMessage);
                }

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Quote selection email sending failed: {ex.Message}");
                return false;
            }
        }

        private string GenerateVerificationEmailHtml(string fullName, string verificationLink)
        {
            return $@"
<!DOCTYPE html>
<html lang='en'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Activate Your Account</title>
    <style>
        * {{
            margin: 0;
            padding: 0;
            box-sizing: border-box;
        }}
        body {{
            font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', 'Helvetica Neue', sans-serif;
            background: linear-gradient(135deg, #f5f7fa 0%, #c3cfe2 100%);
            padding: 20px;
        }}
        .email-container {{
            max-width: 600px;
            margin: 0 auto;
            background: white;
            border-radius: 16px;
            overflow: hidden;
            box-shadow: 0 10px 40px rgba(0, 0, 0, 0.1);
        }}
        .email-header {{
            background: linear-gradient(135deg, #17275F 0%, #2A4A9F 100%);
            padding: 20px 16px;
            text-align: center;
            color: white;
        }}
        .email-header-logo {{
            width: 60px;
            height: 60px;
            background: rgba(255, 255, 255, 0.2);
            border-radius: 12px;
            display: flex;
            align-items: center;
            justify-content: center;
            margin: 0 auto 20px;
            font-size: 32px;
            font-weight: bold;
        }}
        .email-header h1 {{
            font-size: 24px;
            font-weight: 700;
            margin-bottom: 8px;
        }}
        .email-header p {{
            font-size: 14px;
            opacity: 0.9;
        }}
        .email-body {{
            padding: 16px;
        }}
        .greeting {{
            font-size: 16px;
            color: #333;
            margin-bottom: 16px;
            line-height: 1.6;
        }}
        .greeting strong {{
            color: #17275F;
        }}
        .message {{
            font-size: 14px;
            color: #666;
            line-height: 1.8;
            margin-bottom: 16px;
        }}
        .cta-button {{
            display: inline-block;
            background: linear-gradient(135deg, #FF9900 0%, #E68A00 100%);
            color: #ffffff !important;
            padding: 12px 32px;
            border-radius: 8px;
            text-decoration: none !important;
            font-weight: 700;
            font-size: 14px;
            transition: all 0.3s ease;
            box-shadow: 0 4px 15px rgba(255, 153, 0, 0.3);
            display: block;
            text-align: center;
            margin: 16px 0;
            border: none;
        }}
        .cta-button:hover {{
            background: linear-gradient(135deg, #E68A00 0%, #CC7700 100%);
            box-shadow: 0 6px 20px rgba(255, 153, 0, 0.4);
            color: #ffffff !important;
        }}
        .link-text {{
            font-size: 12px;
            color: #999;
            margin-top: 16px;
            word-break: break-all;
        }}
        .link-text a {{
            color: #17275F;
            text-decoration: none;
        }}
        .features {{
            display: none;
        }}
        .email-footer {{
            background: #f8f9fa;
            padding: 16px;
            text-align: center;
            border-top: 1px solid #e5e5e7;
        }}
        .footer-text {{
            font-size: 12px;
            color: #999;
            line-height: 1.6;
        }}
        .footer-links {{
            margin-top: 16px;
        }}
        .footer-links a {{
            color: #17275F;
            text-decoration: none;
            font-size: 12px;
            margin: 0 12px;
        }}
        .support-email {{
            font-size: 12px;
            color: #666;
            margin-top: 16px;
        }}
    </style>
</head>
<body>
    <div class='email-container'>
        <!-- Header -->
        <div class='email-header'>
            <div class='email-header-logo'>🛡️</div>
            <h1>Welcome to Standard Insurance</h1>
            <p>Activate your account to get started</p>
        </div>

        <!-- Body -->
        <div class='email-body'>
            <div class='greeting'>
                Hi <strong>{fullName}</strong>,
            </div>

            <div class='message'>
                Thank you for creating an account with Standard Insurance! We're excited to have you on board. To complete your registration and start exploring our insurance products, please activate your account by clicking the button below.
            </div>

            <a href='{verificationLink}' class='cta-button'>Activate My Account</a>

            <div class='link-text'>
                If the button doesn't work, copy and paste this link in your browser:<br>
                <a href='{verificationLink}'>{verificationLink}</a>
            </div>

            <div class='support-email'>
                Need help? Contact us at <a href='mailto:info@standardinsurance.co.mz'>info@standardinsurance.co.mz</a>
            </div>

            <!-- Security Note -->
            <div class='security-note'>
                🔒 <strong>Security Tip:</strong> This link will expire in 24 hours. If you didn't create this account, please ignore this email.
            </div>

            <!-- Features -->
            <div class='features'>
                <h3>What You Can Do:</h3>
                <ul class='feature-list'>
                    <li>Get instant insurance quotes</li>
                    <li>Compare multiple coverage options</li>
                    <li>Manage your policies online</li>
                    <li>Access 24/7 customer support</li>
                    <li>Receive personalized recommendations</li>
                </ul>
            </div>

            <div class='message'>
                If you have any questions or need assistance, our support team is here to help!
            </div>
        </div>

        <!-- Footer -->
        <div class='email-footer'>
            <div class='footer-text'>
                <strong>Standard Insurance Corretores de Seguros, SA</strong><br>
                Mozambique | Phone: +258-21-50-1020 | Email: info@standardinsurance.co.mz<br>
                <div class='footer-links'>
                    <a href='#'>Privacy Policy</a> | 
                    <a href='#'>Terms of Service</a> | 
                    <a href='#'>Contact Us</a>
                </div>
            </div>
            <div class='footer-text' style='margin-top: 16px; font-size: 11px;'>
                &copy; 2025 Standard Insurance. All rights reserved.
            </div>
        </div>
    </div>
</body>
</html>";
        }

        private string Generate2FAEmailHtml(string displayName, string code, int expiryMinutes)
        {
            return $@"
<!DOCTYPE html>
<html lang='en'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Two-Factor Authentication Code</title>
    <style>
        * {{
            margin: 0;
            padding: 0;
            box-sizing: border-box;
        }}
        body {{
            font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', 'Helvetica Neue', sans-serif;
            background: linear-gradient(135deg, #f5f7fa 0%, #c3cfe2 100%);
            padding: 20px;
        }}
        .email-container {{
            max-width: 600px;
            margin: 0 auto;
            background: white;
            border-radius: 16px;
            overflow: hidden;
            box-shadow: 0 10px 40px rgba(0, 0, 0, 0.1);
        }}
        .email-header {{
            background: linear-gradient(135deg, #17275F 0%, #2A4A9F 100%);
            padding: 20px 16px;
            text-align: center;
            color: white;
        }}
        .email-header-logo {{
            width: 60px;
            height: 60px;
            background: rgba(255, 255, 255, 0.2);
            border-radius: 12px;
            display: flex;
            align-items: center;
            justify-content: center;
            margin: 0 auto 20px;
            font-size: 32px;
            font-weight: bold;
        }}
        .email-header h1 {{
            font-size: 24px;
            font-weight: 700;
            margin-bottom: 8px;
        }}
        .email-header p {{
            font-size: 14px;
            opacity: 0.9;
        }}
        .email-body {{
            padding: 16px;
        }}
        .greeting {{
            font-size: 16px;
            color: #333;
            margin-bottom: 16px;
            line-height: 1.6;
        }}
        .greeting strong {{
            color: #17275F;
        }}
        .message {{
            font-size: 14px;
            color: #666;
            line-height: 1.8;
            margin-bottom: 16px;
        }}
        .code-box {{
            background: linear-gradient(135deg, #f8f9fa 0%, #f0f2f5 100%);
            border: 2px solid #17275F;
            border-radius: 12px;
            padding: 16px;
            text-align: center;
            margin: 16px 0;
        }}
        .code-label {{
            font-size: 12px;
            color: #999;
            text-transform: uppercase;
            letter-spacing: 1px;
            margin-bottom: 12px;
        }}
        .code-display {{
            font-size: 32px;
            font-weight: 700;
            color: #17275F;
            letter-spacing: 6px;
            font-family: 'Courier New', monospace;
            margin: 0;
        }}
        .expiry-info {{
            font-size: 12px;
            color: #666;
            margin-top: 16px;
        }}
        .security-note {{
            background: #e8f4f8;
            border-left: 4px solid #17275F;
            padding: 16px;
            border-radius: 4px;
            margin: 24px 0;
            font-size: 12px;
            color: #333;
        }}
        .security-note strong {{
            color: #17275F;
        }}
        .warning-box {{
            background: #ffebee;
            border-left: 4px solid #d32f2f;
            padding: 16px;
            border-radius: 4px;
            margin: 24px 0;
            font-size: 12px;
            color: #333;
        }}
        .warning-box strong {{
            color: #d32f2f;
        }}
        .email-footer {{
            background: #f8f9fa;
            padding: 30px;
            text-align: center;
            border-top: 1px solid #e5e5e7;
        }}
        .footer-text {{
            font-size: 12px;
            color: #999;
            line-height: 1.6;
        }}
        .footer-links {{
            margin-top: 16px;
        }}
        .footer-links a {{
            color: #17275F;
            text-decoration: none;
            font-size: 12px;
            margin: 0 12px;
        }}
    </style>
</head>
<body>
    <div class='email-container'>
        <!-- Header -->
        <div class='email-header'>
            <div class='email-header-logo'>🔐</div>
            <h1>Verify Your Login</h1>
            <p>Two-Factor Authentication Code</p>
        </div>

        <!-- Body -->
        <div class='email-body'>
            <div class='greeting'>
                Hi <strong>{displayName}</strong>,
            </div>

            <div class='message'>
                You're attempting to sign in to your Standard Insurance account. To complete your login securely, please use the verification code below:
            </div>

            <!-- Code Box -->
            <div class='code-box'>
                <div class='code-label'>Your Verification Code</div>
                <div class='code-display'>{code}</div>
                <div class='expiry-info'>This code will expire in <strong>{expiryMinutes} minutes</strong></div>
            </div>

            <!-- Security Note -->
            <div class='security-note'>
                🔒 <strong>Security Tip:</strong> Never share this code with anyone. Standard Insurance staff will never ask for this code.
            </div>

            <!-- Warning -->
            <div class='warning-box'>
                ⚠️ <strong>Didn't try to log in?</strong> If you didn't attempt to sign in, please ignore this email and secure your account immediately by changing your password.
            </div>

            <div class='message'>
                If you have any questions or need assistance, our support team is here to help!
            </div>
        </div>

        <!-- Footer -->
        <div class='email-footer'>
            <div class='footer-text'>
                <strong>Standard Insurance Corretores de Seguros, SA</strong><br>
                Mozambique | Phone: +258-21-50-1020 | Email: info@standardinsurance.co.mz<br>
                <div class='footer-links'>
                    <a href='#'>Privacy Policy</a> | 
                    <a href='#'>Terms of Service</a> | 
                    <a href='#'>Contact Us</a>
                </div>
            </div>
            <div class='footer-text' style='margin-top: 16px; font-size: 11px;'>
                © 2025 Standard Insurance. All rights reserved.
            </div>
        </div>
    </div>
</body>
</html>";
        }
    }
}
