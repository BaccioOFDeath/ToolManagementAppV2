using System;
using System.Threading;
using System.Threading.Tasks;
using InventoryManagementApp.Interfaces;
using InventoryManagementApp.Services.Core;

namespace InventoryManagementApp.Services.Settings
{
    /// <summary>
    /// Service for managing rental-specific configuration settings.
    /// </summary>
    public class RentalConfigurationService
    {
        private readonly ISettingsService _settingsService;
        private const string DefaultDailyRateKey = "Rental.DefaultDailyRate";
        private const string DefaultLateFeeKey = "Rental.DefaultLateFee";
        private const string ReminderEnabledKey = "Rental.ReminderEnabled";
        private const string EmailEnabledKey = "Email.Enabled";
        private const string SmtpHostKey = "Email.SmtpHost";
        private const string SmtpPortKey = "Email.SmtpPort";
        private const string SmtpUsernameKey = "Email.SmtpUsername";
        private const string SmtpPasswordKey = "Email.SmtpPassword";
        private const string FromEmailKey = "Email.FromEmail";
        private const string FromNameKey = "Email.FromName";
        private const string EnableSslKey = "Email.EnableSsl";
        private const string ContactInfoKey = "Company.ContactInfo";
        private const string CompanyNameKey = "Company.Name";
        private const string CompanyAddressKey = "Company.Address";
        private const string CompanyPhoneKey = "Company.Phone";

        public RentalConfigurationService(ISettingsService settingsService)
        {
            _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
        }

        public async Task<decimal> GetDefaultDailyRateAsync(CancellationToken cancellationToken = default)
        {
            var value = await _settingsService.GetSettingAsync(DefaultDailyRateKey, cancellationToken).ConfigureAwait(false);
            return decimal.TryParse(value, out var rate) ? rate : 25.00m;
        }

        public async Task SetDefaultDailyRateAsync(decimal rate, CancellationToken cancellationToken = default)
        {
            await _settingsService.SaveSettingAsync(DefaultDailyRateKey, rate.ToString("F2"), cancellationToken).ConfigureAwait(false);
        }

        public async Task<decimal> GetDefaultLateFeeAsync(CancellationToken cancellationToken = default)
        {
            var value = await _settingsService.GetSettingAsync(DefaultLateFeeKey, cancellationToken).ConfigureAwait(false);
            return decimal.TryParse(value, out var fee) ? fee : 10.00m;
        }

        public async Task SetDefaultLateFeeAsync(decimal fee, CancellationToken cancellationToken = default)
        {
            await _settingsService.SaveSettingAsync(DefaultLateFeeKey, fee.ToString("F2"), cancellationToken).ConfigureAwait(false);
        }

        public async Task<bool> GetReminderEnabledAsync(CancellationToken cancellationToken = default)
        {
            var value = await _settingsService.GetSettingAsync(ReminderEnabledKey, cancellationToken).ConfigureAwait(false);
            return bool.TryParse(value, out var enabled) ? enabled : true;
        }

        public async Task SetReminderEnabledAsync(bool enabled, CancellationToken cancellationToken = default)
        {
            await _settingsService.SaveSettingAsync(ReminderEnabledKey, enabled.ToString(), cancellationToken).ConfigureAwait(false);
        }

        public async Task<bool> GetEmailEnabledAsync(CancellationToken cancellationToken = default)
        {
            var value = await _settingsService.GetSettingAsync(EmailEnabledKey, cancellationToken).ConfigureAwait(false);
            return bool.TryParse(value, out var enabled) ? enabled : false;
        }

        public async Task SetEmailEnabledAsync(bool enabled, CancellationToken cancellationToken = default)
        {
            await _settingsService.SaveSettingAsync(EmailEnabledKey, enabled.ToString(), cancellationToken).ConfigureAwait(false);
        }

        public async Task<string> GetSmtpHostAsync(CancellationToken cancellationToken = default)
        {
            return await _settingsService.GetSettingAsync(SmtpHostKey, cancellationToken).ConfigureAwait(false) 
                ?? "smtp.example.com";
        }

        public async Task SetSmtpHostAsync(string host, CancellationToken cancellationToken = default)
        {
            await _settingsService.SaveSettingAsync(SmtpHostKey, host, cancellationToken).ConfigureAwait(false);
        }

        public async Task<int> GetSmtpPortAsync(CancellationToken cancellationToken = default)
        {
            var value = await _settingsService.GetSettingAsync(SmtpPortKey, cancellationToken).ConfigureAwait(false);
            return int.TryParse(value, out var port) ? port : 587;
        }

        public async Task SetSmtpPortAsync(int port, CancellationToken cancellationToken = default)
        {
            await _settingsService.SaveSettingAsync(SmtpPortKey, port.ToString(), cancellationToken).ConfigureAwait(false);
        }

        public async Task<string> GetSmtpUsernameAsync(CancellationToken cancellationToken = default)
        {
            return await _settingsService.GetSettingAsync(SmtpUsernameKey, cancellationToken).ConfigureAwait(false) ?? string.Empty;
        }

        public async Task SetSmtpUsernameAsync(string username, CancellationToken cancellationToken = default)
        {
            await _settingsService.SaveSettingAsync(SmtpUsernameKey, username, cancellationToken).ConfigureAwait(false);
        }

        public async Task<string> GetSmtpPasswordAsync(CancellationToken cancellationToken = default)
        {
            return await _settingsService.GetSettingAsync(SmtpPasswordKey, cancellationToken).ConfigureAwait(false) ?? string.Empty;
        }

        public async Task SetSmtpPasswordAsync(string password, CancellationToken cancellationToken = default)
        {
            await _settingsService.SaveSettingAsync(SmtpPasswordKey, password, cancellationToken).ConfigureAwait(false);
        }

        public async Task<string> GetFromEmailAsync(CancellationToken cancellationToken = default)
        {
            return await _settingsService.GetSettingAsync(FromEmailKey, cancellationToken).ConfigureAwait(false) 
                ?? "rentals@example.com";
        }

        public async Task SetFromEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            await _settingsService.SaveSettingAsync(FromEmailKey, email, cancellationToken).ConfigureAwait(false);
        }

        public async Task<string> GetFromNameAsync(CancellationToken cancellationToken = default)
        {
            return await _settingsService.GetSettingAsync(FromNameKey, cancellationToken).ConfigureAwait(false) 
                ?? "Equipment Rentals";
        }

        public async Task SetFromNameAsync(string name, CancellationToken cancellationToken = default)
        {
            await _settingsService.SaveSettingAsync(FromNameKey, name, cancellationToken).ConfigureAwait(false);
        }

        public async Task<bool> GetEnableSslAsync(CancellationToken cancellationToken = default)
        {
            var value = await _settingsService.GetSettingAsync(EnableSslKey, cancellationToken).ConfigureAwait(false);
            return bool.TryParse(value, out var enabled) ? enabled : true;
        }

        public async Task SetEnableSslAsync(bool enabled, CancellationToken cancellationToken = default)
        {
            await _settingsService.SaveSettingAsync(EnableSslKey, enabled.ToString(), cancellationToken).ConfigureAwait(false);
        }

        public async Task<string> GetContactInfoAsync(CancellationToken cancellationToken = default)
        {
            return await _settingsService.GetSettingAsync(ContactInfoKey, cancellationToken).ConfigureAwait(false) 
                ?? "Contact us for more information";
        }

        public async Task SetContactInfoAsync(string info, CancellationToken cancellationToken = default)
        {
            await _settingsService.SaveSettingAsync(ContactInfoKey, info, cancellationToken).ConfigureAwait(false);
        }

        public async Task<string> GetCompanyNameAsync(CancellationToken cancellationToken = default)
        {
            return await _settingsService.GetSettingAsync(CompanyNameKey, cancellationToken).ConfigureAwait(false) 
                ?? "Equipment Rentals";
        }

        public async Task SetCompanyNameAsync(string name, CancellationToken cancellationToken = default)
        {
            await _settingsService.SaveSettingAsync(CompanyNameKey, name, cancellationToken).ConfigureAwait(false);
        }

        public async Task<string> GetCompanyAddressAsync(CancellationToken cancellationToken = default)
        {
            return await _settingsService.GetSettingAsync(CompanyAddressKey, cancellationToken).ConfigureAwait(false) 
                ?? string.Empty;
        }

        public async Task SetCompanyAddressAsync(string address, CancellationToken cancellationToken = default)
        {
            await _settingsService.SaveSettingAsync(CompanyAddressKey, address, cancellationToken).ConfigureAwait(false);
        }

        public async Task<string> GetCompanyPhoneAsync(CancellationToken cancellationToken = default)
        {
            return await _settingsService.GetSettingAsync(CompanyPhoneKey, cancellationToken).ConfigureAwait(false) 
                ?? string.Empty;
        }

        public async Task SetCompanyPhoneAsync(string phone, CancellationToken cancellationToken = default)
        {
            await _settingsService.SaveSettingAsync(CompanyPhoneKey, phone, cancellationToken).ConfigureAwait(false);
        }
    }
}
