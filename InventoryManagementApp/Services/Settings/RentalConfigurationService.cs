using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
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
        private const string QuickRentalDaysKey = "Rental.QuickRentalDays";
        private const string ReminderEnabledKey = "Rental.ReminderEnabled";
        private const string InvoiceEnabledKey = "Rental.InvoiceEnabled";
        private const string EmailEnabledKey = "Email.Enabled";
        private const string SmtpHostKey = "Email.SmtpHost";
        private const string SmtpPortKey = "Email.SmtpPort";
        private const string SmtpUsernameKey = "Email.SmtpUsername";
        private const string SmtpPasswordKey = "Email.SmtpPassword";
        private const string FromEmailKey = "Email.FromEmail";
        private const string FromNameKey = "Email.FromName";
        private const string EnableSslKey = "Email.EnableSsl";
        private const string FromEmailOptionsKey = "Email.FromEmailOptions";
        private const string EmailSignatureKey = "Email.Signature";
        private const string ReminderSubjectTemplateKey = "Email.Template.Reminder.Subject";
        private const string ReminderBodyTemplateKey = "Email.Template.Reminder.Body";
        private const string OverdueSubjectTemplateKey = "Email.Template.Overdue.Subject";
        private const string OverdueBodyTemplateKey = "Email.Template.Overdue.Body";
        private const string ContactInfoKey = "Company.ContactInfo";
        private const string CompanyNameKey = "Company.Name";
        private const string CompanyAddressKey = "Company.Address";
        private const string CompanyPhoneKey = "Company.Phone";
        private const string BackupDirectoryKey = "Backup.Directory";
        private const string SmsProviderKey = "Sms.Provider";
        private const string SmsApiKey = "Sms.ApiKey";
        private const string SmsSenderKey = "Sms.Sender";

        public RentalConfigurationService(ISettingsService settingsService)
        {
            _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
        }

        public async Task<IReadOnlyList<int>> GetQuickRentalDaysAsync(CancellationToken cancellationToken = default)
        {
            var value = await _settingsService.GetSettingAsync(QuickRentalDaysKey, cancellationToken).ConfigureAwait(false);
            return ParseQuickRentalDays(value);
        }

        public async Task SetQuickRentalDaysAsync(IEnumerable<int> days, CancellationToken cancellationToken = default)
        {
            var normalized = NormalizeQuickRentalDays(days);
            await _settingsService.SaveSettingAsync(QuickRentalDaysKey, string.Join(",", normalized), cancellationToken).ConfigureAwait(false);
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

        public static IReadOnlyList<int> ParseQuickRentalDays(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return DefaultQuickRentalDays();
            }

            try
            {
                if (value.TrimStart().StartsWith("[", StringComparison.Ordinal))
                {
                    var parsed = JsonSerializer.Deserialize<List<int>>(value) ?? new List<int>();
                    return NormalizeQuickRentalDays(parsed);
                }
            }
            catch (JsonException)
            {
                return DefaultQuickRentalDays();
            }

            var days = value
                .Split(new[] { ',', ';', '|' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(part => int.TryParse(part, out var day) ? day : 0);

            return NormalizeQuickRentalDays(days);
        }

        public static IReadOnlyList<int> NormalizeQuickRentalDays(IEnumerable<int> days)
        {
            var normalized = days
                .Where(day => day > 0 && day <= 365)
                .Distinct()
                .ToList();

            return normalized.Count == 0 ? DefaultQuickRentalDays() : normalized;
        }

        private static IReadOnlyList<int> DefaultQuickRentalDays() => new[] { 7, 14, 30 };

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

        public async Task<bool> GetInvoiceEnabledAsync(CancellationToken cancellationToken = default)
        {
            var value = await _settingsService.GetSettingAsync(InvoiceEnabledKey, cancellationToken).ConfigureAwait(false);
            return bool.TryParse(value, out var enabled) && enabled;
        }

        public async Task SetInvoiceEnabledAsync(bool enabled, CancellationToken cancellationToken = default)
        {
            await _settingsService.SaveSettingAsync(InvoiceEnabledKey, enabled.ToString(), cancellationToken).ConfigureAwait(false);
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
            var value = await _settingsService.GetSettingAsync(SmtpHostKey, cancellationToken).ConfigureAwait(false);
            return GetSingleLineSettingOrDefault(value, "smtp.example.com");
        }

        public async Task SetSmtpHostAsync(string host, CancellationToken cancellationToken = default)
        {
            await _settingsService.SaveSettingAsync(SmtpHostKey, NormalizeSingleLineSetting(host), cancellationToken).ConfigureAwait(false);
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
            var value = await _settingsService.GetSettingAsync(SmtpUsernameKey, cancellationToken).ConfigureAwait(false);
            return GetSingleLineSettingOrDefault(value, string.Empty);
        }

        public async Task SetSmtpUsernameAsync(string username, CancellationToken cancellationToken = default)
        {
            await _settingsService.SaveSettingAsync(SmtpUsernameKey, NormalizeSingleLineSetting(username), cancellationToken).ConfigureAwait(false);
        }

        public async Task<string> GetSmtpPasswordAsync(CancellationToken cancellationToken = default)
        {
            return await _settingsService.GetSettingAsync(SmtpPasswordKey, cancellationToken).ConfigureAwait(false) ?? string.Empty;
        }

        public async Task SetSmtpPasswordAsync(string password, CancellationToken cancellationToken = default)
        {
            await _settingsService.SaveSettingAsync(SmtpPasswordKey, password ?? string.Empty, cancellationToken).ConfigureAwait(false);
        }

        public async Task<string> GetFromEmailAsync(CancellationToken cancellationToken = default)
        {
            var value = await _settingsService.GetSettingAsync(FromEmailKey, cancellationToken).ConfigureAwait(false);
            return GetSingleLineSettingOrDefault(value, "rentals@example.com");
        }

        public async Task SetFromEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            await _settingsService.SaveSettingAsync(FromEmailKey, NormalizeSingleLineSetting(email), cancellationToken).ConfigureAwait(false);
        }

        public async Task<string> GetFromNameAsync(CancellationToken cancellationToken = default)
        {
            var value = await _settingsService.GetSettingAsync(FromNameKey, cancellationToken).ConfigureAwait(false);
            return GetSingleLineSettingOrDefault(value, "Equipment Rentals");
        }

        public async Task SetFromNameAsync(string name, CancellationToken cancellationToken = default)
        {
            await _settingsService.SaveSettingAsync(FromNameKey, NormalizeSingleLineSetting(name), cancellationToken).ConfigureAwait(false);
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

        public async Task<IReadOnlyList<string>> GetFromEmailOptionsAsync(CancellationToken cancellationToken = default)
        {
            var json = await _settingsService.GetSettingAsync(FromEmailOptionsKey, cancellationToken).ConfigureAwait(false);
            List<string> options;
            if (string.IsNullOrWhiteSpace(json))
            {
                options = new List<string>();
            }
            else
            {
                try
                {
                    options = JsonSerializer.Deserialize<List<string>>(json) ?? new List<string>();
                }
                catch (JsonException)
                {
                    options = new List<string>();
                }
            }

            var currentFromEmail = await GetFromEmailAsync(cancellationToken).ConfigureAwait(false);
            if (!string.IsNullOrWhiteSpace(currentFromEmail))
            {
                options.Insert(0, currentFromEmail);
            }

            return options
                .Where(email => !string.IsNullOrWhiteSpace(email))
                .Select(email => email.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        public async Task SetFromEmailOptionsAsync(IEnumerable<string> options, CancellationToken cancellationToken = default)
        {
            var normalized = (options ?? Enumerable.Empty<string>())
                .Where(email => !string.IsNullOrWhiteSpace(email))
                .Select(email => email.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
            var json = JsonSerializer.Serialize(normalized);
            await _settingsService.SaveSettingAsync(FromEmailOptionsKey, json, cancellationToken).ConfigureAwait(false);
        }

        public async Task<string> GetEmailSignatureAsync(CancellationToken cancellationToken = default)
        {
            var value = await _settingsService.GetSettingAsync(EmailSignatureKey, cancellationToken).ConfigureAwait(false);
            return GetMultilineSettingOrDefault(value, DefaultEmailSignature);
        }

        public async Task SetEmailSignatureAsync(string signature, CancellationToken cancellationToken = default)
        {
            await _settingsService.SaveSettingAsync(EmailSignatureKey, NormalizeMultilineSetting(signature), cancellationToken).ConfigureAwait(false);
        }

        public async Task<string> GetReminderSubjectTemplateAsync(CancellationToken cancellationToken = default)
        {
            var value = await _settingsService.GetSettingAsync(ReminderSubjectTemplateKey, cancellationToken).ConfigureAwait(false);
            return GetSingleLineSettingOrDefault(value, DefaultReminderSubjectTemplate);
        }

        public async Task SetReminderSubjectTemplateAsync(string template, CancellationToken cancellationToken = default)
        {
            await _settingsService.SaveSettingAsync(ReminderSubjectTemplateKey, NormalizeSingleLineSetting(template), cancellationToken).ConfigureAwait(false);
        }

        public async Task<string> GetReminderBodyTemplateAsync(CancellationToken cancellationToken = default)
        {
            var value = await _settingsService.GetSettingAsync(ReminderBodyTemplateKey, cancellationToken).ConfigureAwait(false);
            return GetMultilineSettingOrDefault(value, DefaultReminderBodyTemplate);
        }

        public async Task SetReminderBodyTemplateAsync(string template, CancellationToken cancellationToken = default)
        {
            await _settingsService.SaveSettingAsync(ReminderBodyTemplateKey, NormalizeMultilineSetting(template), cancellationToken).ConfigureAwait(false);
        }

        public async Task<string> GetOverdueSubjectTemplateAsync(CancellationToken cancellationToken = default)
        {
            var value = await _settingsService.GetSettingAsync(OverdueSubjectTemplateKey, cancellationToken).ConfigureAwait(false);
            return GetSingleLineSettingOrDefault(value, DefaultOverdueSubjectTemplate);
        }

        public async Task SetOverdueSubjectTemplateAsync(string template, CancellationToken cancellationToken = default)
        {
            await _settingsService.SaveSettingAsync(OverdueSubjectTemplateKey, NormalizeSingleLineSetting(template), cancellationToken).ConfigureAwait(false);
        }

        public async Task<string> GetOverdueBodyTemplateAsync(CancellationToken cancellationToken = default)
        {
            var value = await _settingsService.GetSettingAsync(OverdueBodyTemplateKey, cancellationToken).ConfigureAwait(false);
            return GetMultilineSettingOrDefault(value, DefaultOverdueBodyTemplate);
        }

        public async Task SetOverdueBodyTemplateAsync(string template, CancellationToken cancellationToken = default)
        {
            await _settingsService.SaveSettingAsync(OverdueBodyTemplateKey, NormalizeMultilineSetting(template), cancellationToken).ConfigureAwait(false);
        }

        public const string DefaultEmailSignature = "Best regards,\nThe Equipment Rental Team";
        public const string DefaultReminderSubjectTemplate = "Reminder: Item {ItemNumber} Due Tomorrow";
        public const string DefaultReminderBodyTemplate = @"Dear {CustomerName},

This is a friendly reminder that the following item is due back tomorrow:

Item Number: {ItemNumber}
Due Date: {DueDate}

Please return the item on or before the due date to avoid late fees.

If you have any questions or need to extend your rental, please contact us at {ContactInfo}.

Thank you for your business!";
        public const string DefaultOverdueSubjectTemplate = "Overdue Rental Notice: Item {ItemNumber}";
        public const string DefaultOverdueBodyTemplate = @"Dear {CustomerName},

Our records show that the following rental item is overdue:

Item Number: {ItemNumber}
Due Date: {DueDate}
Days Overdue: {DaysOverdue}

Please return the item as soon as possible to avoid further late fees.

If you have already returned this item or need to extend your rental, please contact us at {ContactInfo}.";

        public async Task<string> GetContactInfoAsync(CancellationToken cancellationToken = default)
        {
            var value = await _settingsService.GetSettingAsync(ContactInfoKey, cancellationToken).ConfigureAwait(false);
            return GetSingleLineSettingOrDefault(value, "Contact us for more information");
        }

        public async Task SetContactInfoAsync(string info, CancellationToken cancellationToken = default)
        {
            await _settingsService.SaveSettingAsync(ContactInfoKey, NormalizeSingleLineSetting(info), cancellationToken).ConfigureAwait(false);
        }

        public async Task<string> GetCompanyNameAsync(CancellationToken cancellationToken = default)
        {
            var value = await _settingsService.GetSettingAsync(CompanyNameKey, cancellationToken).ConfigureAwait(false);
            return GetSingleLineSettingOrDefault(value, "Equipment Rentals");
        }

        public async Task SetCompanyNameAsync(string name, CancellationToken cancellationToken = default)
        {
            await _settingsService.SaveSettingAsync(CompanyNameKey, NormalizeSingleLineSetting(name), cancellationToken).ConfigureAwait(false);
        }

        public async Task<string> GetCompanyAddressAsync(CancellationToken cancellationToken = default)
        {
            var value = await _settingsService.GetSettingAsync(CompanyAddressKey, cancellationToken).ConfigureAwait(false);
            return GetSingleLineSettingOrDefault(value, string.Empty);
        }

        public async Task SetCompanyAddressAsync(string address, CancellationToken cancellationToken = default)
        {
            await _settingsService.SaveSettingAsync(CompanyAddressKey, NormalizeSingleLineSetting(address), cancellationToken).ConfigureAwait(false);
        }

        public async Task<string> GetCompanyPhoneAsync(CancellationToken cancellationToken = default)
        {
            var value = await _settingsService.GetSettingAsync(CompanyPhoneKey, cancellationToken).ConfigureAwait(false);
            return GetSingleLineSettingOrDefault(value, string.Empty);
        }

        public async Task SetCompanyPhoneAsync(string phone, CancellationToken cancellationToken = default)
        {
            await _settingsService.SaveSettingAsync(CompanyPhoneKey, NormalizeSingleLineSetting(phone), cancellationToken).ConfigureAwait(false);
        }

        public async Task<string> GetBackupDirectoryAsync(CancellationToken cancellationToken = default)
        {
            var value = await _settingsService.GetSettingAsync(BackupDirectoryKey, cancellationToken).ConfigureAwait(false);
            return GetSingleLineSettingOrDefault(value, Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
        }

        public async Task SetBackupDirectoryAsync(string directory, CancellationToken cancellationToken = default)
        {
            await _settingsService.SaveSettingAsync(BackupDirectoryKey, NormalizeSingleLineSetting(directory), cancellationToken).ConfigureAwait(false);
        }

        public async Task<string> GetSmsProviderAsync(CancellationToken cancellationToken = default)
        {
            var value = await _settingsService.GetSettingAsync(SmsProviderKey, cancellationToken).ConfigureAwait(false);
            return GetSingleLineSettingOrDefault(value, "None");
        }

        public async Task SetSmsProviderAsync(string provider, CancellationToken cancellationToken = default)
        {
            await _settingsService.SaveSettingAsync(SmsProviderKey, NormalizeSingleLineSetting(provider), cancellationToken).ConfigureAwait(false);
        }

        public async Task<string> GetSmsApiKeyAsync(CancellationToken cancellationToken = default)
        {
            return await _settingsService.GetSettingAsync(SmsApiKey, cancellationToken).ConfigureAwait(false)
                ?? string.Empty;
        }

        public async Task SetSmsApiKeyAsync(string apiKey, CancellationToken cancellationToken = default)
        {
            await _settingsService.SaveSettingAsync(SmsApiKey, apiKey ?? string.Empty, cancellationToken).ConfigureAwait(false);
        }

        public async Task<string> GetSmsSenderAsync(CancellationToken cancellationToken = default)
        {
            var value = await _settingsService.GetSettingAsync(SmsSenderKey, cancellationToken).ConfigureAwait(false);
            return GetSingleLineSettingOrDefault(value, string.Empty);
        }

        public async Task SetSmsSenderAsync(string sender, CancellationToken cancellationToken = default)
        {
            await _settingsService.SaveSettingAsync(SmsSenderKey, NormalizeSingleLineSetting(sender), cancellationToken).ConfigureAwait(false);
        }

        private static string GetSingleLineSettingOrDefault(string? value, string defaultValue)
        {
            var normalized = NormalizeSingleLineSetting(value);
            return string.IsNullOrWhiteSpace(normalized) ? defaultValue : normalized;
        }

        private static string GetMultilineSettingOrDefault(string? value, string defaultValue)
        {
            var normalized = NormalizeMultilineSetting(value);
            return string.IsNullOrWhiteSpace(normalized) ? defaultValue : normalized;
        }

        private static string NormalizeSingleLineSetting(string? value) => value?.Trim() ?? string.Empty;

        private static string NormalizeMultilineSetting(string? value) => value?.Trim() ?? string.Empty;
    }
}
