using System;
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
        private readonly SemaphoreSlim _checkLock = new(1, 1);
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

            Stop();

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

            if (!await _checkLock.WaitAsync(0).ConfigureAwait(false))
            {
                _logger.LogWarning("Rental reminder check is already running. Skipping overlapping run.");
                return;
            }

            try
            {
                _logger.LogInformation("Checking for rentals due tomorrow...");

                var tomorrow = DateTime.Today.AddDays(1);
                var rentalsDueTomorrowTask = _rentalService.GetActiveRentalsDueOnAsync(tomorrow);
                var emailSignatureTask = GetEmailSignatureAsync();
                var reminderSubjectTemplateTask = GetReminderSubjectTemplateAsync();
                var reminderBodyTemplateTask = GetReminderBodyTemplateAsync();
                var companyNameTask = GetCompanyNameAsync();
                var logoPathTask = GetCompanyLogoPathAsync();

                await Task.WhenAll(
                    rentalsDueTomorrowTask,
                    emailSignatureTask,
                    reminderSubjectTemplateTask,
                    reminderBodyTemplateTask,
                    companyNameTask,
                    logoPathTask).ConfigureAwait(false);

                var rentalsDueTomorrow = await rentalsDueTomorrowTask.ConfigureAwait(false);
                var emailSignature = await emailSignatureTask.ConfigureAwait(false);
                var reminderSubjectTemplate = await reminderSubjectTemplateTask.ConfigureAwait(false);
                var reminderBodyTemplate = await reminderBodyTemplateTask.ConfigureAwait(false);
                var companyName = await companyNameTask.ConfigureAwait(false);
                var logoPath = await logoPathTask.ConfigureAwait(false);

                _logger.LogInformation("Found {Count} rentals due tomorrow", rentalsDueTomorrow.Count);

                var sentCount = 0;
                var skippedCount = 0;
                var failedCount = 0;

                foreach (var rental in rentalsDueTomorrow)
                {
                    try
                    {
                        if (string.IsNullOrWhiteSpace(rental.CustomerEmail))
                        {
                            skippedCount++;
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

                        sentCount++;
                        _logger.LogInformation("Sent reminder for rental {RentalID} to {Email}",
                            rental.RentalID, rental.CustomerEmail);
                    }
                    catch (Exception ex)
                    {
                        failedCount++;
                        _logger.LogError(ex, "Failed to send reminder for rental {RentalID}",
                            rental.RentalID);
                    }
                }

                _logger.LogInformation(
                    "Completed rental reminder run. Due: {DueCount}, Sent: {SentCount}, Skipped: {SkippedCount}, Failed: {FailedCount}",
                    rentalsDueTomorrow.Count,
                    sentCount,
                    skippedCount,
                    failedCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while checking and sending rental reminders");
            }
            finally
            {
                _checkLock.Release();
            }
        }

        private Task<string> GetEmailSignatureAsync()
        {
            return _rentalConfigService == null
                ? Task.FromResult(RentalConfigurationService.DefaultEmailSignature)
                : _rentalConfigService.GetEmailSignatureAsync();
        }

        private Task<string> GetReminderSubjectTemplateAsync()
        {
            return _rentalConfigService == null
                ? Task.FromResult(RentalConfigurationService.DefaultReminderSubjectTemplate)
                : _rentalConfigService.GetReminderSubjectTemplateAsync();
        }

        private Task<string> GetReminderBodyTemplateAsync()
        {
            return _rentalConfigService == null
                ? Task.FromResult(RentalConfigurationService.DefaultReminderBodyTemplate)
                : _rentalConfigService.GetReminderBodyTemplateAsync();
        }

        private Task<string> GetCompanyNameAsync()
        {
            return _rentalConfigService == null
                ? Task.FromResult("Equipment Rentals")
                : _rentalConfigService.GetCompanyNameAsync();
        }

        private Task<string?> GetCompanyLogoPathAsync()
        {
            return _settingsService == null
                ? Task.FromResult<string?>(null)
                : _settingsService.GetSettingAsync("CompanyLogoPath");
        }

        public void Dispose()
        {
            if (_disposed) return;
            Stop();
            _checkLock.Dispose();
            _disposed = true;
            GC.SuppressFinalize(this);
        }
    }
}
