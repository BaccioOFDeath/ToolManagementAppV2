using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using InventoryManagementApp.Interfaces;
using InventoryManagementApp.Services.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace InventoryManagementApp.Services.Notifications
{
    /// <summary>
    /// Service that runs periodically to send rental return reminders.
    /// Sends reminders at 2:30 PM for items due the next day.
    /// </summary>
    public class RentalReminderService : IDisposable
    {
        private readonly IRentalService _rentalService;
        private readonly EmailService? _emailService;
        private readonly RentalConfigurationService? _rentalConfigService;
        private readonly ISettingsService? _settingsService;
        private readonly ILogger<RentalReminderService> _logger;
        private readonly string _contactInfo;
        private System.Threading.Timer? _timer;
        private bool _disposed;

        public RentalReminderService(
            IRentalService rentalService,
            EmailService? emailService,
            string contactInfo = "your rental team",
            ILogger<RentalReminderService>? logger = null,
            RentalConfigurationService? rentalConfigService = null,
            ISettingsService? settingsService = null)
        {
            _rentalService = rentalService ?? throw new ArgumentNullException(nameof(rentalService));
            _emailService = emailService;
            _rentalConfigService = rentalConfigService;
            _settingsService = settingsService;
            _contactInfo = contactInfo;
            _logger = logger ?? NullLogger<RentalReminderService>.Instance;
        }

        /// <summary>
        /// Starts the reminder service, which will check daily at 2:30 PM.
        /// </summary>
        public void Start()
        {
            if (_emailService == null)
            {
                _logger.LogWarning("Email service not configured. Rental reminders will not be sent.");
                return;
            }

            var now = DateTime.Now;
            var scheduledTime = new DateTime(now.Year, now.Month, now.Day, 14, 30, 0);
            
            if (now > scheduledTime)
            {
                scheduledTime = scheduledTime.AddDays(1);
            }

            var initialDelay = scheduledTime - now;
            
            _logger.LogInformation("Rental reminder service starting. Next check at {Time}", scheduledTime);

            _timer = new System.Threading.Timer(
                async _ => await CheckAndSendRemindersAsync().ConfigureAwait(false),
                null,
                initialDelay,
                TimeSpan.FromDays(1));
        }

        /// <summary>
        /// Stops the reminder service.
        /// </summary>
        public void Stop()
        {
            _timer?.Dispose();
            _timer = null;
            _logger.LogInformation("Rental reminder service stopped");
        }

        /// <summary>
        /// Manually trigger a check for rentals due tomorrow and send reminders.
        /// </summary>
        public async Task CheckAndSendRemindersAsync()
        {
            if (_emailService == null)
            {
                _logger.LogWarning("Email service not configured. Cannot send reminders.");
                return;
            }

            try
            {
                _logger.LogInformation("Checking for rentals due tomorrow...");

                var activeRentals = await _rentalService.GetActiveRentalsAsync().ConfigureAwait(false);
                var tomorrow = DateTime.Today.AddDays(1);
                var emailSignature = _rentalConfigService == null ? RentalConfigurationService.DefaultEmailSignature : await _rentalConfigService.GetEmailSignatureAsync().ConfigureAwait(false);
                var reminderSubjectTemplate = _rentalConfigService == null ? RentalConfigurationService.DefaultReminderSubjectTemplate : await _rentalConfigService.GetReminderSubjectTemplateAsync().ConfigureAwait(false);
                var reminderBodyTemplate = _rentalConfigService == null ? RentalConfigurationService.DefaultReminderBodyTemplate : await _rentalConfigService.GetReminderBodyTemplateAsync().ConfigureAwait(false);
                var companyName = _rentalConfigService == null ? "Equipment Rentals" : await _rentalConfigService.GetCompanyNameAsync().ConfigureAwait(false);
                var logoPath = _settingsService == null ? null : await _settingsService.GetSettingAsync("CompanyLogoPath").ConfigureAwait(false);
                
                var rentalsDueTomorrow = activeRentals
                    .Where(r => r.DueDate.Date == tomorrow)
                    .ToList();

                _logger.LogInformation("Found {Count} rentals due tomorrow", rentalsDueTomorrow.Count);

                foreach (var rental in rentalsDueTomorrow)
                {
                    try
                    {
                        if (string.IsNullOrWhiteSpace(rental.CustomerEmail))
                        {
                            _logger.LogWarning("Rental {RentalID} has no customer email, skipping reminder",
                                rental.RentalID);
                            continue;
                        }

                        await _emailService.SendRentalReminderAsync(
                            rental.CustomerEmail,
                            rental.CustomerName,
                            rental.ItemNumber,
                            rental.DueDate,
                            _contactInfo,
                            companyName,
                            emailSignature,
                            logoPath,
                            rental.ImagePath,
                            reminderSubjectTemplate,
                            reminderBodyTemplate).ConfigureAwait(false);

                        _logger.LogInformation("Sent reminder for rental {RentalID} to {Email}",
                            rental.RentalID, rental.CustomerEmail);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to send reminder for rental {RentalID}",
                            rental.RentalID);
                    }
                }

                _logger.LogInformation("Completed sending {Count} rental reminders", rentalsDueTomorrow.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while checking and sending rental reminders");
            }
        }

        public void Dispose()
        {
            if (_disposed) return;
            Stop();
            _disposed = true;
            GC.SuppressFinalize(this);
        }
    }
}
