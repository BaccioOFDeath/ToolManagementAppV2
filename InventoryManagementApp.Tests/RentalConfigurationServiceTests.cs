using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using InventoryManagementApp.Services.Core;
using InventoryManagementApp.Services.Settings;
using Xunit;

namespace InventoryManagementApp.Tests
{
    public class RentalConfigurationServiceTests : IDisposable
    {
        private readonly string _testDbPath;
        private readonly DatabaseService _dbService;
        private readonly SettingsService _settingsService;
        private readonly RentalConfigurationService _configService;

        public RentalConfigurationServiceTests()
        {
            _testDbPath = Path.Combine(Path.GetTempPath(), $"test_rental_config_{Guid.NewGuid()}.db");
            _dbService = new DatabaseService(_testDbPath);
            _settingsService = new SettingsService(_dbService);
            _configService = new RentalConfigurationService(_settingsService);
        }

        public void Dispose()
        {
            _dbService?.Dispose();
            if (File.Exists(_testDbPath))
                File.Delete(_testDbPath);
        }

        [Fact]
        public void Constructor_WithNullSettingsService_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new RentalConfigurationService(null!));
        }

        [Fact]
        public async Task GetDefaultDailyRateAsync_WithNoSetting_ReturnsDefaultValue()
        {
            var rate = await _configService.GetDefaultDailyRateAsync();
            
            Assert.Equal(25.00m, rate);
        }

        [Fact]
        public async Task SetAndGetDefaultDailyRate_StoresAndRetrievesValue()
        {
            await _configService.SetDefaultDailyRateAsync(35.50m);
            var rate = await _configService.GetDefaultDailyRateAsync();
            
            Assert.Equal(35.50m, rate);
        }

        [Fact]
        public async Task GetDefaultLateFeeAsync_WithNoSetting_ReturnsDefaultValue()
        {
            var fee = await _configService.GetDefaultLateFeeAsync();
            
            Assert.Equal(10.00m, fee);
        }

        [Fact]
        public async Task SetAndGetDefaultLateFee_StoresAndRetrievesValue()
        {
            await _configService.SetDefaultLateFeeAsync(20.00m);
            var fee = await _configService.GetDefaultLateFeeAsync();
            
            Assert.Equal(20.00m, fee);
        }

        [Fact]
        public async Task GetQuickRentalDaysAsync_WithNoSetting_ReturnsDefaultValues()
        {
            var days = await _configService.GetQuickRentalDaysAsync();

            Assert.Equal(new[] { 7, 14, 30 }, days);
        }

        [Fact]
        public async Task SetAndGetQuickRentalDays_StoresNormalizedValues()
        {
            await _configService.SetQuickRentalDaysAsync(new[] { 3, 7, 7, 14, 0, 400 });
            var days = await _configService.GetQuickRentalDaysAsync();

            Assert.Equal(new[] { 3, 7, 14 }, days);
        }

        [Fact]
        public void ParseQuickRentalDays_WithInvalidValue_ReturnsDefaultValues()
        {
            var days = RentalConfigurationService.ParseQuickRentalDays("not a number");

            Assert.Equal(new[] { 7, 14, 30 }, days);
        }

        [Fact]
        public async Task GetReminderEnabledAsync_WithNoSetting_ReturnsTrue()
        {
            var enabled = await _configService.GetReminderEnabledAsync();
            
            Assert.True(enabled);
        }

        [Fact]
        public async Task SetAndGetReminderEnabled_StoresAndRetrievesValue()
        {
            await _configService.SetReminderEnabledAsync(false);
            var enabled = await _configService.GetReminderEnabledAsync();
            
            Assert.False(enabled);
        }

        [Fact]
        public async Task GetInvoiceEnabledAsync_WithNoSetting_ReturnsFalse()
        {
            var enabled = await _configService.GetInvoiceEnabledAsync();

            Assert.False(enabled);
        }

        [Fact]
        public async Task SetAndGetInvoiceEnabled_StoresAndRetrievesValue()
        {
            await _configService.SetInvoiceEnabledAsync(true);
            var enabled = await _configService.GetInvoiceEnabledAsync();

            Assert.True(enabled);
        }

        [Fact]
        public async Task GetEmailEnabledAsync_WithNoSetting_ReturnsFalse()
        {
            var enabled = await _configService.GetEmailEnabledAsync();
            
            Assert.False(enabled);
        }

        [Fact]
        public async Task SetAndGetEmailEnabled_StoresAndRetrievesValue()
        {
            await _configService.SetEmailEnabledAsync(true);
            var enabled = await _configService.GetEmailEnabledAsync();
            
            Assert.True(enabled);
        }

        [Fact]
        public async Task SetAndGetSmtpHost_StoresAndRetrievesValue()
        {
            await _configService.SetSmtpHostAsync("smtp.gmail.com");
            var host = await _configService.GetSmtpHostAsync();
            
            Assert.Equal("smtp.gmail.com", host);
        }

        [Fact]
        public async Task SetAndGetSmtpPort_StoresAndRetrievesValue()
        {
            await _configService.SetSmtpPortAsync(465);
            var port = await _configService.GetSmtpPortAsync();
            
            Assert.Equal(465, port);
        }

        [Fact]
        public async Task SetAndGetFromEmail_StoresAndRetrievesValue()
        {
            await _configService.SetFromEmailAsync("test@example.com");
            var email = await _configService.GetFromEmailAsync();
            
            Assert.Equal("test@example.com", email);
        }

        [Fact]
        public async Task SetAndGetCompanyName_StoresAndRetrievesValue()
        {
            await _configService.SetCompanyNameAsync("My Company");
            var name = await _configService.GetCompanyNameAsync();
            
            Assert.Equal("My Company", name);
        }

        [Fact]
        public async Task GetCompanyAddress_WithNoSetting_ReturnsEmptyString()
        {
            var address = await _configService.GetCompanyAddressAsync();
            
            Assert.Equal(string.Empty, address);
        }

        [Fact]
        public async Task SetAndGetContactInfo_StoresAndRetrievesValue()
        {
            await _configService.SetContactInfoAsync("Call (555) 123-4567");
            var info = await _configService.GetContactInfoAsync();
            
            Assert.Equal("Call (555) 123-4567", info);
        }

        [Fact]
        public async Task SetAndGetBackupDirectory_StoresAndRetrievesValue()
        {
            var directory = Path.Combine(Path.GetTempPath(), "inventory-backups");
            await _configService.SetBackupDirectoryAsync(directory);
            var stored = await _configService.GetBackupDirectoryAsync();

            Assert.Equal(directory, stored);
        }

        [Fact]
        public async Task SetAndGetSmsSettings_StoresAndRetrievesValues()
        {
            await _configService.SetSmsProviderAsync("Twilio");
            await _configService.SetSmsApiKeyAsync("api-key");
            await _configService.SetSmsSenderAsync("+15551234567");

            var provider = await _configService.GetSmsProviderAsync();
            var apiKey = await _configService.GetSmsApiKeyAsync();
            var sender = await _configService.GetSmsSenderAsync();

            Assert.Equal("Twilio", provider);
            Assert.Equal("api-key", apiKey);
            Assert.Equal("+15551234567", sender);
        }

        [Fact]
        public async Task SetAndGetFromEmailOptions_StoresAndRetrievesValues()
        {
            var options = new[] { "first@example.com", "second@example.com", "first@example.com" };
            await _configService.SetFromEmailOptionsAsync(options);

            var stored = await _configService.GetFromEmailOptionsAsync();

            Assert.Contains("first@example.com", stored, StringComparer.OrdinalIgnoreCase);
            Assert.Contains("second@example.com", stored, StringComparer.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task SetAndGetEmailTemplates_StoresAndRetrievesValues()
        {
            await _configService.SetEmailSignatureAsync("Regards,\nService Desk");
            await _configService.SetReminderSubjectTemplateAsync("Reminder {ItemNumber}");
            await _configService.SetReminderBodyTemplateAsync("Reminder body {CustomerName}");
            await _configService.SetOverdueSubjectTemplateAsync("Overdue {ItemNumber}");
            await _configService.SetOverdueBodyTemplateAsync("Overdue body {DaysOverdue}");

            Assert.Equal("Regards,\nService Desk", await _configService.GetEmailSignatureAsync());
            Assert.Equal("Reminder {ItemNumber}", await _configService.GetReminderSubjectTemplateAsync());
            Assert.Equal("Reminder body {CustomerName}", await _configService.GetReminderBodyTemplateAsync());
            Assert.Equal("Overdue {ItemNumber}", await _configService.GetOverdueSubjectTemplateAsync());
            Assert.Equal("Overdue body {DaysOverdue}", await _configService.GetOverdueBodyTemplateAsync());
        }
    }
}
