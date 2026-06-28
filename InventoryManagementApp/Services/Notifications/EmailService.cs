using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace InventoryManagementApp.Services.Notifications
{
    /// <summary>
    /// Service for sending email notifications for rentals and reminders.
    /// </summary>
    public class EmailService : IDisposable
    {
        private readonly ILogger<EmailService> _logger;
        private readonly string _smtpHost;
        private readonly int _smtpPort;
        private readonly string _smtpUsername;
        private readonly string _smtpPassword;
        private readonly string _fromEmail;
        private readonly string _fromName;
        private readonly bool _enableSsl;
        private SmtpClient? _smtpClient;
        private bool _disposed;

        public EmailService(
            string smtpHost,
            int smtpPort,
            string smtpUsername,
            string smtpPassword,
            string fromEmail,
            string fromName,
            bool enableSsl = true,
            ILogger<EmailService>? logger = null)
        {
            _smtpHost = smtpHost ?? throw new ArgumentNullException(nameof(smtpHost));
            _smtpPort = smtpPort;
            _smtpUsername = smtpUsername ?? throw new ArgumentNullException(nameof(smtpUsername));
            _smtpPassword = smtpPassword ?? throw new ArgumentNullException(nameof(smtpPassword));
            _fromEmail = fromEmail ?? throw new ArgumentNullException(nameof(fromEmail));
            _fromName = fromName;
            _enableSsl = enableSsl;
            _logger = logger ?? NullLogger<EmailService>.Instance;
        }

        private SmtpClient GetSmtpClient()
        {
            if (_smtpClient == null)
            {
                _smtpClient = new SmtpClient(_smtpHost, _smtpPort)
                {
                    EnableSsl = _enableSsl,
                    Credentials = new NetworkCredential(_smtpUsername, _smtpPassword),
                    Timeout = 30000
                };
            }
            return _smtpClient;
        }

        /// <summary>
        /// Sends a rental return reminder email to a customer.
        /// </summary>
        public async Task SendRentalReminderAsync(
            string customerEmail,
            string customerName,
            string itemNumber,
            DateTime dueDate,
            string contactInfo,
            string? companyName = null,
            string? signature = null,
            string? logoPath = null,
            string? itemImagePath = null,
            string? subjectTemplate = null,
            string? bodyTemplate = null)
        {
            if (string.IsNullOrWhiteSpace(customerEmail))
            {
                _logger.LogWarning("Cannot send reminder: customer email is empty");
                return;
            }

            try
            {
                var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    ["CustomerName"] = customerName,
                    ["ItemNumber"] = itemNumber,
                    ["DueDate"] = dueDate.ToString("yyyy-MM-dd"),
                    ["DaysOverdue"] = Math.Max(0, (DateTime.Today.Date - dueDate.Date).Days).ToString(),
                    ["ContactInfo"] = contactInfo
                };

                var subject = ApplyTemplate(subjectTemplate ?? "Reminder: Item {ItemNumber} Due Tomorrow", values);
                var body = ApplyTemplate(bodyTemplate ?? $@"Dear {{CustomerName}},

This is a friendly reminder that the following item is due back tomorrow:

Item Number: {{ItemNumber}}
Due Date: {{DueDate}}

Please return the item on or before the due date to avoid late fees.

If you have any questions or need to extend your rental, please contact us at {{ContactInfo}}.

Thank you for your business!", values);

                using var message = new MailMessage
                {
                    From = new MailAddress(_fromEmail, _fromName),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = false
                };
                message.To.Add(customerEmail);
                AddBrandedHtmlView(message, subject, body, companyName, signature, logoPath, itemImagePath);

                var client = GetSmtpClient();
                await client.SendMailAsync(message).ConfigureAwait(false);
                
                _logger.LogInformation("Sent rental reminder to {Email} for item {ItemNumber}", 
                    customerEmail, itemNumber);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send rental reminder to {Email}", customerEmail);
                throw;
            }
        }

        /// <summary>
        /// Sends a general notification email.
        /// </summary>
        public async Task SendEmailAsync(string toEmail, string subject, string body, bool isHtml = false)
        {
            if (string.IsNullOrWhiteSpace(toEmail))
            {
                _logger.LogWarning("Cannot send email: recipient email is empty");
                return;
            }

            try
            {
                using var message = new MailMessage
                {
                    From = new MailAddress(_fromEmail, _fromName),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = isHtml
                };
                message.To.Add(toEmail);

                var client = GetSmtpClient();
                await client.SendMailAsync(message).ConfigureAwait(false);
                
                _logger.LogInformation("Sent email to {Email} with subject: {Subject}", toEmail, subject);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {Email}", toEmail);
                throw;
            }
        }

        public async Task SendBrandedEmailAsync(
            string toEmail,
            string subject,
            string body,
            string? companyName = null,
            string? signature = null,
            string? logoPath = null,
            string? itemImagePath = null)
        {
            if (string.IsNullOrWhiteSpace(toEmail))
            {
                _logger.LogWarning("Cannot send email: recipient email is empty");
                return;
            }

            try
            {
                using var message = new MailMessage
                {
                    From = new MailAddress(_fromEmail, _fromName),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = false
                };
                message.To.Add(toEmail);
                AddBrandedHtmlView(message, subject, body, companyName, signature, logoPath, itemImagePath);

                var client = GetSmtpClient();
                await client.SendMailAsync(message).ConfigureAwait(false);

                _logger.LogInformation("Sent branded email to {Email} with subject: {Subject}", toEmail, subject);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send branded email to {Email}", toEmail);
                throw;
            }
        }

        internal static string ApplyTemplate(string template, IReadOnlyDictionary<string, string> values)
        {
            var result = template ?? string.Empty;
            foreach (var pair in values)
            {
                result = result.Replace("{" + pair.Key + "}", pair.Value ?? string.Empty, StringComparison.OrdinalIgnoreCase);
            }

            return result;
        }

        private static void AddBrandedHtmlView(
            MailMessage message,
            string title,
            string body,
            string? companyName,
            string? signature,
            string? logoPath,
            string? itemImagePath)
        {
            var itemImageContentId = TryResolveFile(itemImagePath) != null ? "item-image" : null;
            var html = BuildBrandedHtml(title, body, companyName, signature, null, itemImageContentId);
            var view = AlternateView.CreateAlternateViewFromString(html, Encoding.UTF8, MediaTypeNames.Text.Html);

            AddLinkedResource(view, itemImagePath, itemImageContentId);
            message.AlternateViews.Add(view);
        }

        private static void AddLinkedResource(AlternateView view, string? path, string? contentId)
        {
            var resolved = TryResolveFile(path);
            if (resolved == null || string.IsNullOrWhiteSpace(contentId))
            {
                return;
            }

            var resource = new LinkedResource(resolved)
            {
                ContentId = contentId,
                TransferEncoding = TransferEncoding.Base64
            };
            view.LinkedResources.Add(resource);
        }

        internal static string BuildBrandedHtml(
            string title,
            string body,
            string? companyName,
            string? signature,
            string? logoContentId,
            string? itemImageContentId)
        {
            var encodedCompanyName = HtmlEncoder.Default.Encode(string.IsNullOrWhiteSpace(companyName) ? "Equipment Rentals" : companyName);
            var encodedTitle = HtmlEncoder.Default.Encode(title);
            var encodedPreheader = HtmlEncoder.Default.Encode(BuildPreheader(body));
            var bodyHtml = ConvertPlainTextToHtml(body);
            var signatureHtml = string.IsNullOrWhiteSpace(signature)
                ? string.Empty
                : $"<div class=\"signature\" style=\"max-width:520px;margin:28px auto 0;padding-top:18px;border-top:1px solid #e2e4e8;color:#6b7280;font-size:14px;text-align:center;\">{ConvertPlainTextToHtml(signature)}</div>";
            var itemImageHtml = string.IsNullOrWhiteSpace(itemImageContentId)
                ? string.Empty
                : $@"<table role=""presentation"" width=""300"" align=""center"" cellpadding=""0"" cellspacing=""0"" style=""width:300px;margin:0 auto 28px;background:#f1f2f5;border:1px solid #e2e4e8;border-radius:14px;"">
            <tr>
              <td align=""center"" style=""padding:16px;text-align:center;"">
                <img src=""cid:{itemImageContentId}"" width=""190"" alt=""Rental item"" style=""display:block;width:190px;max-width:190px;height:auto;max-height:190px;margin:0 auto;border:0;outline:none;text-decoration:none;"" />
              </td>
            </tr>
          </table>";

            return $@"<!doctype html>
<html>
<head>
  <meta charset=""utf-8"">
  <style>
    body {{ margin:0; padding:0; background:#f7f8fa; color:#1c1c1e; font-family:Inter, Segoe UI, Arial, Helvetica, sans-serif; font-size:16px; }}
    .preheader {{ display:none; max-height:0; overflow:hidden; opacity:0; color:transparent; }}
    p {{ margin:0 0 16px; line-height:1.72; }}
    .body {{ color:#4b5560; font-size:16px; }}
    .facts {{ margin:26px auto; border:1px solid #e2e4e8; border-radius:12px; background:#ffffff; overflow:hidden; text-align:left; }}
    .fact-row {{ padding:14px 18px; border-top:1px solid #e2e4e8; }}
    .fact-row:first-child {{ border-top:0; }}
    .fact-label {{ display:inline-block; min-width:124px; color:#6b7280; font-size:11px; font-weight:800; text-transform:uppercase; letter-spacing:.8px; }}
    .fact-value {{ color:#1c1c1e; font-weight:800; }}
    .cta {{ margin:28px 0 8px; text-align:center; }}
    .cta span {{ display:inline-block; background:#f5b700; color:#0f0f0f; padding:14px 26px; border-radius:100px; font-size:14px; font-weight:800; }}
  </style>
</head>
<body style=""margin:0;padding:0;background:#f7f8fa;color:#1c1c1e;font-family:Inter, Segoe UI, Arial, Helvetica, sans-serif;font-size:16px;"">
  <div class=""preheader"">{encodedPreheader}</div>
  <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""width:100%;background:#f7f8fa;margin:0;padding:0;"">
    <tr>
      <td align=""center"" style=""padding:42px 24px;"">
        <table role=""presentation"" width=""640"" cellpadding=""0"" cellspacing=""0"" style=""width:640px;max-width:640px;background:#ffffff;border:1px solid #e2e4e8;border-radius:20px;overflow:hidden;box-shadow:0 12px 40px rgba(0,0,0,0.10);"">
          <tr>
            <td style=""height:8px;background:#f5b700;font-size:0;line-height:0;"">&nbsp;</td>
          </tr>
          <tr>
            <td align=""center"" style=""background:#0f0f0f;padding:38px 48px 42px;text-align:center;"">
              <div style=""color:#ffffff;font-size:18px;font-weight:900;margin:0 0 18px;"">{encodedCompanyName}</div>
              <div style=""display:inline-block;background:#2b240d;border:1px solid rgba(245,183,0,0.36);color:#f5b700;border-radius:100px;padding:7px 16px;font-size:12px;font-weight:800;letter-spacing:.7px;text-transform:uppercase;"">Rental item notice</div>
              <h1 style=""max-width:520px;margin:20px auto 0;color:#ffffff;font-size:38px;line-height:1.1;font-weight:900;letter-spacing:-.5px;"">{encodedTitle}</h1>
            </td>
          </tr>
          <tr>
            <td align=""center"" style=""padding:34px 52px 40px;text-align:center;"">
              <table role=""presentation"" width=""520"" align=""center"" cellpadding=""0"" cellspacing=""0"" style=""width:520px;max-width:520px;margin:0 auto;"">
                <tr>
                  <td align=""center"" style=""text-align:center;"">
                    {itemImageHtml}
                    <div class=""body"" style=""color:#4b5560;font-size:16px;text-align:left;"">{bodyHtml}</div>
                    <div class=""cta"" style=""margin:28px 0 8px;text-align:center;""><span style=""display:inline-block;background:#f5b700;color:#0f0f0f;padding:14px 26px;border-radius:100px;font-size:14px;font-weight:800;"">Contact the rental team</span></div>
                  </td>
                </tr>
              </table>
              {signatureHtml}
            </td>
          </tr>
          <tr>
            <td align=""center"" style=""padding:22px 48px 28px;background:#1c1c1e;color:rgba(255,255,255,0.62);font-size:12px;line-height:1.55;text-align:center;"">This message was sent by {encodedCompanyName}. Please reply to this email if the rental record needs to be updated.</td>
          </tr>
        </table>
      </td>
    </tr>
  </table>
</body>
</html>";
        }

        private static string BuildPreheader(string value)
        {
            var firstLine = (value ?? string.Empty)
                .Replace("\r\n", "\n", StringComparison.Ordinal)
                .Split('\n', StringSplitOptions.RemoveEmptyEntries)
                .Select(line => line.Trim())
                .FirstOrDefault(line => line.Length > 0);

            return string.IsNullOrWhiteSpace(firstLine)
                ? "Rental item reminder from your equipment team."
                : firstLine;
        }

        private static string ConvertPlainTextToHtml(string value)
        {
            var paragraphs = (value ?? string.Empty)
                .Replace("\r\n", "\n", StringComparison.Ordinal)
                .Split("\n\n", StringSplitOptions.None)
                .Select(paragraph => paragraph.Trim('\n'));

            return string.Join(Environment.NewLine, paragraphs.Select(ConvertParagraphToHtml));
        }

        private static string ConvertParagraphToHtml(string paragraph)
        {
            var lines = paragraph
                .Split('\n', StringSplitOptions.RemoveEmptyEntries)
                .Select(line => line.Trim())
                .Where(line => line.Length > 0)
                .ToList();

            if (lines.Count > 0 && lines.All(IsFactLine))
            {
                var rows = lines.Select(line =>
                {
                    var separator = line.IndexOf(':', StringComparison.Ordinal);
                    var label = HtmlEncoder.Default.Encode(line[..separator].Trim());
                    var value = HtmlEncoder.Default.Encode(line[(separator + 1)..].Trim());
                    return $"<div class=\"fact-row\"><span class=\"fact-label\">{label}</span><span class=\"fact-value\">{value}</span></div>";
                });

                return $"<div class=\"facts\">{string.Join(Environment.NewLine, rows)}</div>";
            }

            return $"<p>{HtmlEncoder.Default.Encode(paragraph).Replace("\n", "<br>", StringComparison.Ordinal)}</p>";
        }

        private static bool IsFactLine(string line)
        {
            var separator = line.IndexOf(':', StringComparison.Ordinal);
            if (separator <= 0) return false;

            var label = line[..separator].Trim();
            return string.Equals(label, "Item Number", StringComparison.OrdinalIgnoreCase)
                || string.Equals(label, "Due Date", StringComparison.OrdinalIgnoreCase)
                || string.Equals(label, "Days Overdue", StringComparison.OrdinalIgnoreCase)
                || string.Equals(label, "Customer", StringComparison.OrdinalIgnoreCase)
                || string.Equals(label, "Contact", StringComparison.OrdinalIgnoreCase)
                || string.Equals(label, "Rental Date", StringComparison.OrdinalIgnoreCase)
                || string.Equals(label, "Return Date", StringComparison.OrdinalIgnoreCase);
        }

        private static string? TryResolveFile(string? path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return null;
            }

            var trimmed = path.Trim();
            if (Path.IsPathRooted(trimmed) && File.Exists(trimmed))
            {
                return trimmed;
            }

            var appRelative = Path.Combine(AppContext.BaseDirectory, trimmed);
            return File.Exists(appRelative) ? appRelative : null;
        }

        public void Dispose()
        {
            if (_disposed) return;
            _smtpClient?.Dispose();
            _disposed = true;
            GC.SuppressFinalize(this);
        }
    }
}
