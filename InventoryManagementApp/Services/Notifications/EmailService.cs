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
            var isOverdue = title?.IndexOf("overdue", StringComparison.OrdinalIgnoreCase) >= 0 ||
                            body?.IndexOf("Days Overdue", StringComparison.OrdinalIgnoreCase) >= 0;
            var noticeLabel = isOverdue ? "Return required" : "Rental item notice";
            var noticeColor = isOverdue ? "#b91c1c" : "#f5b700";
            var noticeBackground = isOverdue ? "#fff1f2" : "#fff7d6";
            var noticeText = isOverdue
                ? "Please return the item or contact the rental team to arrange an extension."
                : "A reminder from the rental desk so the item is ready for the next booking.";
            var bodyHtml = ConvertPlainTextToHtml(body ?? string.Empty);
            var signatureHtml = string.IsNullOrWhiteSpace(signature)
                ? string.Empty
                : $"<div class=\"signature\" style=\"margin:28px 0 0;padding-top:18px;border-top:1px solid #e2e4e8;color:#6b7280;font-size:14px;line-height:1.55;\">{ConvertPlainTextToHtml(signature!)}</div>";
            var itemImageHtml = string.IsNullOrWhiteSpace(itemImageContentId)
                ? string.Empty
                : $@"<table role=""presentation"" width=""240"" align=""center"" cellpadding=""0"" cellspacing=""0"" style=""width:240px;margin:0 auto 24px;background:#f7f8fa;border:1px solid #e2e4e8;border-radius:14px;"">
            <tr>
              <td align=""center"" style=""padding:14px;text-align:center;"">
                <img src=""cid:{itemImageContentId}"" width=""180"" alt=""Rental item"" style=""display:block;width:180px;max-width:180px;height:auto;max-height:180px;margin:0 auto;border:0;outline:none;text-decoration:none;border-radius:10px;"" />
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
    p {{ margin:0 0 15px; line-height:1.66; }}
    .body {{ color:#374151; font-size:15px; }}
    .facts {{ margin:22px 0; border:1px solid #e2e4e8; border-radius:12px; background:#ffffff; overflow:hidden; text-align:left; }}
    .fact-label {{ color:#6b7280; font-size:12px; font-weight:700; }}
    .fact-value {{ color:#1c1c1e; font-weight:800; }}
    .cta {{ margin:26px 0 4px; text-align:left; }}
    .cta span {{ display:inline-block; background:#f5b700; color:#0f0f0f; padding:13px 20px; border-radius:100px; font-size:14px; font-weight:800; }}
  </style>
</head>
<body style=""margin:0;padding:0;background:#f7f8fa;color:#1c1c1e;font-family:Inter, Segoe UI, Arial, Helvetica, sans-serif;font-size:16px;"">
  <div class=""preheader"">{encodedPreheader}</div>
  <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""width:100%;background:#f7f8fa;margin:0;padding:0;"">
    <tr>
      <td align=""center"" style=""padding:32px 16px;"">
        <table role=""presentation"" width=""600"" cellpadding=""0"" cellspacing=""0"" style=""width:600px;max-width:600px;background:#ffffff;border:1px solid #e2e4e8;border-radius:16px;overflow:hidden;box-shadow:0 10px 30px rgba(15,15,15,0.08);"">
          <tr>
            <td style=""height:8px;background:#f5b700;font-size:0;line-height:0;"">&nbsp;</td>
          </tr>
          <tr>
            <td style=""background:#0f0f0f;padding:26px 30px 28px;text-align:left;"">
              <div style=""color:#ffffff;font-size:18px;font-weight:900;margin:0 0 14px;"">{encodedCompanyName}</div>
              <div style=""display:inline-block;background:{noticeBackground};border:1px solid {noticeColor};color:{noticeColor};border-radius:100px;padding:6px 13px;font-size:11px;font-weight:800;letter-spacing:.7px;text-transform:uppercase;"">{HtmlEncoder.Default.Encode(noticeLabel)}</div>
              <h1 style=""margin:18px 0 0;color:#ffffff;font-size:28px;line-height:1.18;font-weight:900;"">{encodedTitle}</h1>
              <p style=""margin:10px 0 0;color:#d1d5db;font-size:14px;line-height:1.5;"">{HtmlEncoder.Default.Encode(noticeText)}</p>
            </td>
          </tr>
          <tr>
            <td align=""center"" style=""padding:30px;text-align:left;"">
              <table role=""presentation"" width=""540"" align=""center"" cellpadding=""0"" cellspacing=""0"" style=""width:540px;max-width:540px;margin:0 auto;"">
                <tr>
                  <td style=""text-align:left;"">
                    {itemImageHtml}
                    <div class=""body"" style=""color:#374151;font-size:15px;text-align:left;"">{bodyHtml}</div>
                    <table role=""presentation"" cellpadding=""0"" cellspacing=""0"" style=""margin:24px 0 4px;"">
                      <tr>
                        <td style=""background:#f5b700;border-radius:100px;"">
                          <span style=""display:inline-block;color:#0f0f0f;padding:13px 20px;font-size:14px;font-weight:800;"">Contact the rental team</span>
                        </td>
                      </tr>
                    </table>
                    {signatureHtml}
                  </td>
                </tr>
              </table>
            </td>
          </tr>
          <tr>
            <td style=""padding:18px 30px;background:#f7f8fa;border-top:1px solid #e2e4e8;color:#6b7280;font-size:12px;line-height:1.55;text-align:left;"">
              <strong style=""color:#1c1c1e;"">{encodedCompanyName}</strong><br>
              Please reply to this email if the rental record needs to be updated.
            </td>
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
                var rows = lines.Select((line, index) =>
                {
                    var separator = line.IndexOf(':', StringComparison.Ordinal);
                    var label = HtmlEncoder.Default.Encode(line[..separator].Trim());
                    var value = HtmlEncoder.Default.Encode(line[(separator + 1)..].Trim());
                    var topBorder = index == 0 ? "border-top:0;" : "border-top:1px solid #e2e4e8;";
                    return $@"<tr>
              <td style=""width:38%;padding:13px 16px;background:#f7f8fa;{topBorder}color:#6b7280;font-size:12px;font-weight:700;"">{label}</td>
              <td style=""padding:13px 16px;{topBorder}color:#1c1c1e;font-size:15px;font-weight:800;"">{value}</td>
            </tr>";
                });

                return $@"<table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" class=""facts"" style=""width:100%;margin:22px 0;border:1px solid #e2e4e8;border-radius:12px;background:#ffffff;overflow:hidden;text-align:left;border-collapse:separate;border-spacing:0;"">
          {string.Join(Environment.NewLine, rows)}
        </table>";
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
