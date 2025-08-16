using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ToolManagementAppV2.Services.Core;
using ToolManagementAppV2.Services.Settings;
using ToolManagementAppV2.Utilities.Helpers;
using ToolManagementAppV2.Interfaces;
using Xunit;

namespace ToolManagementAppV2.Tests.Utilities
{
    public class SecurityHelperTests
    {
        [Fact]
        public void HashPassword_UsesConfiguredIterations()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var dbService = new DatabaseService(dbPath);
                ISettingsService settings = new SettingsService(dbService);
                settings.SavePasswordIterations(5);
                SecurityHelper.SettingsService = settings;

                var saltBytes = Encoding.UTF8.GetBytes("1234567890ABCDEF");
                var salt = Convert.ToBase64String(saltBytes);

                using var pbkdf2 = new Rfc2898DeriveBytes("secret", saltBytes, 5, HashAlgorithmName.SHA256);
                var expected = Convert.ToBase64String(pbkdf2.GetBytes(32));
                var actual = SecurityHelper.HashPassword("secret", salt);
                Assert.Equal(expected, actual);
            }
            finally
            {
                SecurityHelper.SettingsService = null;
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }

        [Fact]
        public void HashPassword_DefaultIterationsWhenNotConfigured()
        {
            SecurityHelper.SettingsService = null;
            var saltBytes = Encoding.UTF8.GetBytes("1234567890ABCDEF");
            var salt = Convert.ToBase64String(saltBytes);

            using var pbkdf2 = new Rfc2898DeriveBytes("secret", saltBytes, 100_000, HashAlgorithmName.SHA256);
            var expected = Convert.ToBase64String(pbkdf2.GetBytes(32));
            var actual = SecurityHelper.HashPassword("secret", salt);
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void VerifyPassword_ReturnsTrueForValidHash()
        {
            SecurityHelper.SettingsService = null;
            var hash = SecurityHelper.HashPassword("secret", out var salt);
            var result = SecurityHelper.VerifyPassword("secret", salt, hash);
            Assert.True(result);
        }

        [Fact]
        public void HashPassword_OnlyFetchesIterationsOnceAcrossThreads()
        {
            var settings = new CountingSettingsService(5);
            SecurityHelper.SettingsService = settings;

            var saltBytes = Encoding.UTF8.GetBytes("1234567890ABCDEF");
            var salt = Convert.ToBase64String(saltBytes);

            Parallel.For(0, 20, _ =>
            {
                SecurityHelper.HashPassword("secret", salt);
            });

            Assert.Equal(1, settings.Counter);
            SecurityHelper.SettingsService = null;
        }

        [Fact]
        public void HashPassword_FallsBackToAsyncIterations()
        {
            var settings = new AsyncOnlySettingsService(7);
            SecurityHelper.SettingsService = settings;

            var saltBytes = Encoding.UTF8.GetBytes("1234567890ABCDEF");
            var salt = Convert.ToBase64String(saltBytes);

            using var pbkdf2 = new Rfc2898DeriveBytes("secret", saltBytes, 7, HashAlgorithmName.SHA256);
            var expected = Convert.ToBase64String(pbkdf2.GetBytes(32));
            var actual = SecurityHelper.HashPassword("secret", salt);
            Assert.Equal(expected, actual);

            SecurityHelper.SettingsService = null;
        }

        [Fact]
        public void HashPasswordAsync_DoesNotDeadlock()
        {
            var settings = new AsyncOnlySettingsService(7);
            SecurityHelper.SettingsService = settings;

            var task = SecurityHelper.HashPasswordAsync("secret");
            var completed = task.Wait(1000);
            Assert.True(completed, "HashPasswordAsync timed out, possible deadlock.");

            SecurityHelper.SettingsService = null;
        }

        class CountingSettingsService : ISettingsService
        {
            int _counter;
            readonly int _iterations;

            public CountingSettingsService(int iterations) => _iterations = iterations;

            public int Counter => _counter;

            public int GetPasswordIterations()
            {
                Interlocked.Increment(ref _counter);
                return _iterations;
            }

            public Task<int> GetPasswordIterationsAsync()
            {
                Interlocked.Increment(ref _counter);
                return Task.FromResult(_iterations);
            }

            public void SaveSetting(string key, string value) => throw new NotImplementedException();
            public Task SaveSettingAsync(string key, string value) => throw new NotImplementedException();
            public string? GetSetting(string key) => throw new NotImplementedException();
            public Task<string?> GetSettingAsync(string key) => throw new NotImplementedException();
            public Dictionary<string, string> GetAllSettings() => throw new NotImplementedException();
            public Task<Dictionary<string, string>> GetAllSettingsAsync() => throw new NotImplementedException();
            public void UpdateSettings(Dictionary<string, string> settings) => throw new NotImplementedException();
            public Task UpdateSettingsAsync(Dictionary<string, string> settings) => throw new NotImplementedException();
            public void DeleteSetting(string key) => throw new NotImplementedException();
            public Task DeleteSettingAsync(string key) => throw new NotImplementedException();
            public IEnumerable<string> GetScannerIpAddresses() => throw new NotImplementedException();
            public Task<IEnumerable<string>> GetScannerIpAddressesAsync() => throw new NotImplementedException();
            public IEnumerable<string> SaveScannerIpAddresses(IEnumerable<string>? ipAddresses) => throw new NotImplementedException();
            public Task<IEnumerable<string>> SaveScannerIpAddressesAsync(IEnumerable<string>? ipAddresses) => throw new NotImplementedException();
            public void SavePasswordIterations(int iterations) => throw new NotImplementedException();
            public Task SavePasswordIterationsAsync(int iterations) => throw new NotImplementedException();
        }

        class AsyncOnlySettingsService : ISettingsService
        {
            readonly int _iterations;

            public AsyncOnlySettingsService(int iterations) => _iterations = iterations;

            public int GetPasswordIterations() => 0;
            public Task<int> GetPasswordIterationsAsync() => Task.FromResult(_iterations);

            public void SaveSetting(string key, string value) => throw new NotImplementedException();
            public Task SaveSettingAsync(string key, string value) => throw new NotImplementedException();
            public string? GetSetting(string key) => throw new NotImplementedException();
            public Task<string?> GetSettingAsync(string key) => throw new NotImplementedException();
            public Dictionary<string, string> GetAllSettings() => throw new NotImplementedException();
            public Task<Dictionary<string, string>> GetAllSettingsAsync() => throw new NotImplementedException();
            public void UpdateSettings(Dictionary<string, string> settings) => throw new NotImplementedException();
            public Task UpdateSettingsAsync(Dictionary<string, string> settings) => throw new NotImplementedException();
            public void DeleteSetting(string key) => throw new NotImplementedException();
            public Task DeleteSettingAsync(string key) => throw new NotImplementedException();
            public IEnumerable<string> GetScannerIpAddresses() => throw new NotImplementedException();
            public Task<IEnumerable<string>> GetScannerIpAddressesAsync() => throw new NotImplementedException();
            public IEnumerable<string> SaveScannerIpAddresses(IEnumerable<string>? ipAddresses) => throw new NotImplementedException();
            public Task<IEnumerable<string>> SaveScannerIpAddressesAsync(IEnumerable<string>? ipAddresses) => throw new NotImplementedException();
            public void SavePasswordIterations(int iterations) => throw new NotImplementedException();
            public Task SavePasswordIterationsAsync(int iterations) => throw new NotImplementedException();
        }
    }
}
