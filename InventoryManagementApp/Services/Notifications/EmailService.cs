using System;
using System.Net;
using System.Net.Mail;
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
            string contactInfo)
        {
            if (string.IsNullOrWhiteSpace(customerEmail))
            {
                _logger.LogWarning("Cannot send reminder: customer email is empty");
                return;
            }

            try
            {
                var subject = $"Reminder: Item {itemNumber} Due Tomorrow";
                var body = $@"Dear {customerName},

This is a friendly reminder that the following item is due back tomorrow:

Item Number: {itemNumber}
Due Date: {dueDate:yyyy-MM-dd}

Please return the item on or before the due date to avoid late fees.

If you have any questions or need to extend your rental, please contact us at {contactInfo}.

Thank you for your business!

Best regards,
The Equipment Rental Team";

                using var message = new MailMessage
                {
                    From = new MailAddress(_fromEmail, _fromName),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = false
                };
                message.To.Add(customerEmail);

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

        public void Dispose()
        {
            if (_disposed) return;
            _smtpClient?.Dispose();
            _disposed = true;
        }
    }
}
