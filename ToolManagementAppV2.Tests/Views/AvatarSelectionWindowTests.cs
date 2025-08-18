using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using ToolManagementAppV2.Interfaces;
using ToolManagementAppV2.Views;
using Xunit;

namespace ToolManagementAppV2.Tests.Views
{
    public class AvatarSelectionWindowTests
    {
        [Fact]
        public void Loaded_SetsTitle_FromSettings()
        {
            Exception? threadException = null;

            var thread = new Thread(() =>
            {
                try
                {
                    var settings = new StubSettingsService { AppName = "TestApp" };
                    var logger = new TestLogger<AvatarSelectionWindow>();
                    var window = new AvatarSelectionWindow(settings, logger);

                    window.RaiseEvent(new RoutedEventArgs(FrameworkElement.LoadedEvent));

                    Assert.Equal("TestApp – Select Avatar", window.Title);
                }
                catch (Exception ex)
                {
                    threadException = ex;
                }
            });

            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();

            if (threadException != null)
                throw threadException;
        }

        [Fact]
        public void Loaded_LogsError_WhenSettingFails()
        {
            Exception? threadException = null;

            var thread = new Thread(() =>
            {
                try
                {
                    var settings = new FailingSettingsService();
                    var logger = new TestLogger<AvatarSelectionWindow>();
                    var window = new AvatarSelectionWindow(settings, logger);

                    var initialTitle = window.Title;

                    window.RaiseEvent(new RoutedEventArgs(FrameworkElement.LoadedEvent));

                    Assert.Equal(initialTitle, window.Title);
                    Assert.Contains(logger.Entries, e => e.Level == LogLevel.Error);
                }
                catch (Exception ex)
                {
                    threadException = ex;
                }
            });

            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();

            if (threadException != null)
                throw threadException;
        }

        class StubSettingsService : ISettingsService
        {
            public string? AppName { get; set; }

            public Task<string?> GetSettingAsync(string key, CancellationToken cancellationToken = default)
                => Task.FromResult(AppName);

            public Task SaveSettingAsync(string key, string value, CancellationToken cancellationToken = default)
                => Task.CompletedTask;
            public Task<Dictionary<string, string>> GetAllSettingsAsync(CancellationToken cancellationToken = default)
                => Task.FromResult(new Dictionary<string, string>());
            public Task UpdateSettingsAsync(Dictionary<string, string> settings, CancellationToken cancellationToken = default)
                => Task.CompletedTask;
            public Task DeleteSettingAsync(string key, CancellationToken cancellationToken = default)
                => Task.CompletedTask;
            public Task<IEnumerable<string>> GetScannerIpAddressesAsync(CancellationToken cancellationToken = default)
                => Task.FromResult<IEnumerable<string>>(Array.Empty<string>());
            public Task<IEnumerable<string>> SaveScannerIpAddressesAsync(IEnumerable<string>? ipAddresses, CancellationToken cancellationToken = default)
                => Task.FromResult<IEnumerable<string>>(Array.Empty<string>());
            public Task<int> GetPasswordIterationsAsync(CancellationToken cancellationToken = default)
                => Task.FromResult(0);
            public Task SavePasswordIterationsAsync(int iterations, CancellationToken cancellationToken = default)
                => Task.CompletedTask;
            public Task<int> GetAutoLogoutMinutesAsync(CancellationToken cancellationToken = default)
                => Task.FromResult(0);
            public Task SaveAutoLogoutMinutesAsync(int minutes, CancellationToken cancellationToken = default)
                => Task.CompletedTask;
        }

        class FailingSettingsService : ISettingsService
        {
            public Task<string?> GetSettingAsync(string key, CancellationToken cancellationToken = default)
                => Task.FromException<string?>(new InvalidOperationException("failure"));

            public Task SaveSettingAsync(string key, string value, CancellationToken cancellationToken = default)
                => Task.CompletedTask;
            public Task<Dictionary<string, string>> GetAllSettingsAsync(CancellationToken cancellationToken = default)
                => Task.FromResult(new Dictionary<string, string>());
            public Task UpdateSettingsAsync(Dictionary<string, string> settings, CancellationToken cancellationToken = default)
                => Task.CompletedTask;
            public Task DeleteSettingAsync(string key, CancellationToken cancellationToken = default)
                => Task.CompletedTask;
            public Task<IEnumerable<string>> GetScannerIpAddressesAsync(CancellationToken cancellationToken = default)
                => Task.FromResult<IEnumerable<string>>(Array.Empty<string>());
            public Task<IEnumerable<string>> SaveScannerIpAddressesAsync(IEnumerable<string>? ipAddresses, CancellationToken cancellationToken = default)
                => Task.FromResult<IEnumerable<string>>(Array.Empty<string>());
            public Task<int> GetPasswordIterationsAsync(CancellationToken cancellationToken = default)
                => Task.FromResult(0);
            public Task SavePasswordIterationsAsync(int iterations, CancellationToken cancellationToken = default)
                => Task.CompletedTask;
            public Task<int> GetAutoLogoutMinutesAsync(CancellationToken cancellationToken = default)
                => Task.FromResult(0);
            public Task SaveAutoLogoutMinutesAsync(int minutes, CancellationToken cancellationToken = default)
                => Task.CompletedTask;
        }
    }
}

