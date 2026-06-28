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
            var logoContentId = TryResolveFile(logoPath) != null ? "company-logo" : null;
            var itemImageContentId = TryResolveFile(itemImagePath) != null ? "item-image" : null;
            var html = BuildBrandedHtml(title, body, companyName, signature, logoContentId, itemImageContentId);
            var view = AlternateView.CreateAlternateViewFromString(html, Encoding.UTF8, MediaTypeNames.Text.Html);

            AddLinkedResource(view, logoPath, logoContentId);
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
            var bodyHtml = ConvertPlainTextToHtml(body);
            var signatureHtml = string.IsNullOrWhiteSpace(signature)
                ? string.Empty
                : $"<div class=\"signature\">{ConvertPlainTextToHtml(signature)}</div>";
            var logoHtml = string.IsNullOrWhiteSpace(logoContentId)
                ? string.Empty
                : $"<img class=\"logo\" src=\"cid:{logoContentId}\" alt=\"{encodedCompanyName} logo\" />";
            var itemImageHtml = string.IsNullOrWhiteSpace(itemImageContentId)
                ? string.Empty
                : $"<div class=\"item-image-wrap\"><img class=\"item-image\" src=\"cid:{itemImageContentId}\" alt=\"Rental item\" /></div>";

            return $@"<!doctype html>
<html>
<head>
  <meta charset=""utf-8"">
  <style>
    body {{ margin:0; padding:0; background:#f3f4f6; color:#1f2937; font-family:Segoe UI, Arial, sans-serif; }}
    .shell {{ max-width:680px; margin:0 auto; padding:24px; }}
    .card {{ background:#ffffff; border:1px solid #d7dde5; border-radius:8px; overflow:hidden; }}
    .header {{ background:#111827; color:#ffffff; padding:18px 22px; display:flex; align-items:center; gap:14px; }}
    .logo {{ max-width:132px; max-height:64px; object-fit:contain; background:#ffffff; border-radius:4px; padding:5px; }}
    .brand {{ font-size:18px; font-weight:700; letter-spacing:0; }}
    .content {{ padding:22px; }}
    h1 {{ margin:0 0 14px; font-size:22px; line-height:1.25; color:#111827; }}
    p {{ margin:0 0 12px; line-height:1.55; }}
    .item-image-wrap {{ margin:0 0 18px; border:1px solid #d7dde5; border-radius:6px; background:#f9fafb; padding:10px; text-align:center; }}
    .item-image {{ max-width:100%; max-height:220px; object-fit:contain; }}
    .signature {{ margin-top:20px; padding-top:14px; border-top:1px solid #e5e7eb; color:#374151; }}
    .footer {{ padding:14px 22px; background:#f9fafb; color:#6b7280; font-size:12px; border-top:1px solid #e5e7eb; }}
  </style>
</head>
<body>
  <div class=""shell"">
    <div class=""card"">
      <div class=""header"">{logoHtml}<div class=""brand"">{encodedCompanyName}</div></div>
      <div class=""content"">
        <h1>{encodedTitle}</h1>
        {itemImageHtml}
        <div>{bodyHtml}</div>
        {signatureHtml}
      </div>
      <div class=""footer"">This message was sent by {encodedCompanyName}.</div>
    </div>
  </div>
</body>
</html>";
        }

        private static string ConvertPlainTextToHtml(string value)
        {
            var paragraphs = (value ?? string.Empty)
                .Replace("\r\n", "\n", StringComparison.Ordinal)
                .Split("\n\n", StringSplitOptions.None)
                .Select(paragraph => paragraph.Trim('\n'));

            return string.Join(Environment.NewLine, paragraphs.Select(paragraph =>
                $"<p>{HtmlEncoder.Default.Encode(paragraph).Replace("\n", "<br>", StringComparison.Ordinal)}</p>"));
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
