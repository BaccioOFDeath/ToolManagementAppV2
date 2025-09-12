using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using DeviceManagementApp.Interfaces;
using DeviceManagementApp.Models;
using DeviceManagementApp.ViewModels;
using DeviceManagementApp.Views.Pages;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace DeviceManagementApp.Tests
{
    public class DeviceSettingsPageTests
    {
        private sealed class StubSettingsService : ISettingsService
        {
            private readonly Dictionary<string, string> _settings = new();
            public event EventHandler<IDictionary<ItemDetailField, bool>>? ItemDetailVisibilityChanged;
            public Task SaveSettingAsync(string key, string value, CancellationToken cancellationToken = default)
            { _settings[key] = value; return Task.CompletedTask; }
            public Task<string?> GetSettingAsync(string? key, CancellationToken cancellationToken = default)
            { return Task.FromResult(key != null && _settings.TryGetValue(key, out var v) ? v : null); }
            public Task<Dictionary<string, string>> GetAllSettingsAsync(CancellationToken cancellationToken = default)
            { return Task.FromResult(new Dictionary<string, string>(_settings)); }
            public Task UpdateSettingsAsync(Dictionary<string, string> settings, CancellationToken cancellationToken = default)
            { foreach (var kv in settings) _settings[kv.Key] = kv.Value; return Task.CompletedTask; }
            public Task DeleteSettingAsync(string key, CancellationToken cancellationToken = default)
            { _settings.Remove(key); return Task.CompletedTask; }
            public Task<string?> GetThemeAsync(CancellationToken cancellationToken = default) => Task.FromResult<string?>(null);
            public Task SaveThemeAsync(string theme, CancellationToken cancellationToken = default) => Task.CompletedTask;
            public Task<int> GetPasswordIterationsAsync(CancellationToken cancellationToken = default) => Task.FromResult(0);
            public Task SavePasswordIterationsAsync(int iterations, CancellationToken cancellationToken = default) => Task.CompletedTask;
            public Task<int> GetAutoLogoutMinutesAsync(CancellationToken cancellationToken = default) => Task.FromResult(0);
            public Task SaveAutoLogoutMinutesAsync(int minutes, CancellationToken cancellationToken = default) => Task.CompletedTask;
            public Task<string> GetItemLabelSingularAsync(CancellationToken cancellationToken = default) => Task.FromResult(string.Empty);
            public Task SaveItemLabelSingularAsync(string label, CancellationToken cancellationToken = default) => Task.CompletedTask;
            public Task<string> GetItemLabelPluralAsync(CancellationToken cancellationToken = default) => Task.FromResult(string.Empty);
            public Task SaveItemLabelPluralAsync(string label, CancellationToken cancellationToken = default) => Task.CompletedTask;
            public Task<IDictionary<ItemDetailField, bool>> GetItemDetailVisibilityAsync(CancellationToken cancellationToken = default)
                => Task.FromResult<IDictionary<ItemDetailField, bool>>(new Dictionary<ItemDetailField, bool>());
            public Task SaveItemDetailVisibilityAsync(IDictionary<ItemDetailField, bool> visibility, CancellationToken cancellationToken = default)
            { ItemDetailVisibilityChanged?.Invoke(this, visibility); return Task.CompletedTask; }
        }

        private sealed class DummyDialogService : IDialogService
        {
            public void ShowInfo(string message, string title) { }
            public bool ShowConfirmation(string message, string title) => false;
        }

        [Fact]
        public void DeviceSettingsPage_Loaded_InitializesViewModel()
        {
            Exception? threadEx = null;
            DeviceSettingsViewModel? vm = null;
            Button? saveButton = null;

            var thread = new Thread(() =>
            {
                try
                {
                    var app = new Application();
                    app.Resources.MergedDictionaries.Add(new ResourceDictionary
                    {
                        Source = new Uri("pack://application:,,,/DeviceManagementApp;component/Resources/Styles.xaml", UriKind.Absolute)
                    });

                    var settings = new StubSettingsService();
                    settings.SaveSettingAsync("DeviceDiscovery_Subnets", "10.0.0.0/24").Wait();
                    var config = new ConfigurationBuilder().Build();
                    vm = new DeviceSettingsViewModel(settings, config, new DummyDialogService());
                    var page = new DeviceSettingsPage { DataContext = vm };
                    page.ApplyTemplate();
                    page.RaiseEvent(new RoutedEventArgs(FrameworkElement.LoadedEvent));
                    saveButton = (Button)page.FindName("SaveButton");
                    Application.Current?.Shutdown();
                }
                catch (Exception ex)
                {
                    threadEx = ex;
                }
            });

            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();

            if (threadEx != null) throw threadEx;
            Assert.Equal("10.0.0.0/24", vm?.Subnets);
            Assert.Equal(vm?.SaveCommand, saveButton?.Command);
        }
    }
}
