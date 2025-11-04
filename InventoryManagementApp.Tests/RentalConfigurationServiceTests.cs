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
    }
}
