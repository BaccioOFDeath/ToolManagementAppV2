using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using InventoryManagementApp.Services.Core;
using InventoryManagementApp.Services.Settings;
using InventoryManagementApp.Utilities.Helpers;
using InventoryManagementApp.Interfaces;
using Xunit;

namespace InventoryManagementApp.Tests.Utilities
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
                settings.SavePasswordIterationsAsync(5).GetAwaiter().GetResult();
                SecurityHelper.SettingsService = settings;
                SecurityHelper.GetIterationsAsync().GetAwaiter().GetResult();

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
        public async Task HashPasswordAsync_ReturnsExpectedHash()
        {
            SecurityHelper.SettingsService = null;
            var saltBytes = Encoding.UTF8.GetBytes("1234567890ABCDEF");
            var salt = Convert.ToBase64String(saltBytes);

            using var pbkdf2 = new Rfc2898DeriveBytes("secret", saltBytes, 100_000, HashAlgorithmName.SHA256);
            var expected = Convert.ToBase64String(pbkdf2.GetBytes(32));
            var actual = await SecurityHelper.HashPasswordAsync("secret", salt);
            Assert.Equal(expected, actual);
        }

        [Fact]
        public async Task VerifyPasswordAsync_ReturnsTrueForValidHash()
        {
            SecurityHelper.SettingsService = null;
            var (hash, salt) = await SecurityHelper.HashPasswordAsync("secret");
            var result = await SecurityHelper.VerifyPasswordAsync("secret", salt, hash);
            Assert.True(result);
        }

        [Theory]
        [InlineData(100_000)]
        [InlineData(7)]
        public async Task VerifyPasswordAsync_SucceedsWithKnownHash(int iterations)
        {
            try
            {
                SecurityHelper.SettingsService = iterations == 100_000
                    ? null
                    : new AsyncOnlySettingsService(iterations);

                var saltBytes = Encoding.UTF8.GetBytes("1234567890ABCDEF");
                var salt = Convert.ToBase64String(saltBytes);

                using var pbkdf2 = new Rfc2898DeriveBytes("secret", saltBytes, iterations, HashAlgorithmName.SHA256);
                var hash = Convert.ToBase64String(pbkdf2.GetBytes(32));

                var result = await SecurityHelper.VerifyPasswordAsync("secret", salt, hash);
                Assert.True(result);
            }
            finally
            {
                SecurityHelper.SettingsService = null;
            }
        }

        [Fact]
        public void HashPassword_OnlyFetchesIterationsOnceAcrossThreads()
        {
            var settings = new CountingSettingsService(5);
            SecurityHelper.SettingsService = settings;
            SecurityHelper.GetIterationsAsync().GetAwaiter().GetResult();

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
            SecurityHelper.GetIterationsAsync().GetAwaiter().GetResult();

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

            public Task<int> GetPasswordIterationsAsync(CancellationToken cancellationToken = default)
            {
                Interlocked.Increment(ref _counter);
                return Task.FromResult(_iterations);
            }
            public Task SaveSettingAsync(string key, string value, CancellationToken cancellationToken = default) => throw new NotImplementedException();
            public Task<string?> GetSettingAsync(string key, CancellationToken cancellationToken = default) => throw new NotImplementedException();
            public Task<Dictionary<string, string>> GetAllSettingsAsync(CancellationToken cancellationToken = default) => throw new NotImplementedException();
            public Task UpdateSettingsAsync(Dictionary<string, string> settings, CancellationToken cancellationToken = default) => throw new NotImplementedException();
            public Task DeleteSettingAsync(string key, CancellationToken cancellationToken = default) => throw new NotImplementedException();
            public Task<IEnumerable<string>> GetScannerIpAddressesAsync(CancellationToken cancellationToken = default) => throw new NotImplementedException();
            public Task<IEnumerable<string>> SaveScannerIpAddressesAsync(IEnumerable<string>? ipAddresses, CancellationToken cancellationToken = default) => throw new NotImplementedException();
            public Task SavePasswordIterationsAsync(int iterations, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        }

        class AsyncOnlySettingsService : ISettingsService
        {
            readonly int _iterations;

            public AsyncOnlySettingsService(int iterations) => _iterations = iterations;
            public Task<int> GetPasswordIterationsAsync(CancellationToken cancellationToken = default) => Task.FromResult(_iterations);

            public Task SaveSettingAsync(string key, string value, CancellationToken cancellationToken = default) => throw new NotImplementedException();
            public Task<string?> GetSettingAsync(string key, CancellationToken cancellationToken = default) => throw new NotImplementedException();
            public Task<Dictionary<string, string>> GetAllSettingsAsync(CancellationToken cancellationToken = default) => throw new NotImplementedException();
            public Task UpdateSettingsAsync(Dictionary<string, string> settings, CancellationToken cancellationToken = default) => throw new NotImplementedException();
            public Task DeleteSettingAsync(string key, CancellationToken cancellationToken = default) => throw new NotImplementedException();
            public Task<IEnumerable<string>> GetScannerIpAddressesAsync(CancellationToken cancellationToken = default) => throw new NotImplementedException();
            public Task<IEnumerable<string>> SaveScannerIpAddressesAsync(IEnumerable<string>? ipAddresses, CancellationToken cancellationToken = default) => throw new NotImplementedException();
            public Task SavePasswordIterationsAsync(int iterations, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        }
    }
}
