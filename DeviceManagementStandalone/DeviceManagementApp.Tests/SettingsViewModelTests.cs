using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DeviceManagementApp.Interfaces;
using DeviceManagementApp.Models;
using DeviceManagementApp.ViewModels;
using Xunit;

namespace DeviceManagementApp.Tests
{
    public class SettingsViewModelTests
    {
        private sealed class StubSettingsService : ISettingsService
        {
            public string ThemeSaved = string.Empty;
            public event EventHandler<IDictionary<ItemDetailField, bool>>? ItemDetailVisibilityChanged;
            public Task SaveSettingAsync(string key, string value, CancellationToken cancellationToken = default)
            { Settings[key] = value; return Task.CompletedTask; }
            public Task<string?> GetSettingAsync(string? key, CancellationToken cancellationToken = default)
            { return Task.FromResult(key != null && Settings.TryGetValue(key, out var v) ? v : null); }
            public Task<Dictionary<string, string>> GetAllSettingsAsync(CancellationToken cancellationToken = default)
            { return Task.FromResult(new Dictionary<string, string>(Settings)); }
            public Task UpdateSettingsAsync(Dictionary<string, string> settings, CancellationToken cancellationToken = default)
            { foreach (var kv in settings) Settings[kv.Key] = kv.Value; return Task.CompletedTask; }
            public Task DeleteSettingAsync(string key, CancellationToken cancellationToken = default)
            { Settings.Remove(key); return Task.CompletedTask; }
            public Task<string?> GetThemeAsync(CancellationToken cancellationToken = default) => Task.FromResult<string?>(_theme);
            public Task SaveThemeAsync(string theme, CancellationToken cancellationToken = default)
            { ThemeSaved = theme; _theme = theme; return Task.CompletedTask; }
            public Task<int> GetPasswordIterationsAsync(CancellationToken cancellationToken = default) => Task.FromResult(0);
            public Task SavePasswordIterationsAsync(int iterations, CancellationToken cancellationToken = default) => Task.CompletedTask;
            public Task<int> GetAutoLogoutMinutesAsync(CancellationToken cancellationToken = default) => Task.FromResult(0);
            public Task SaveAutoLogoutMinutesAsync(int minutes, CancellationToken cancellationToken = default) => Task.CompletedTask;
            public Task<string> GetItemLabelSingularAsync(CancellationToken cancellationToken = default) => Task.FromResult("Device");
            public Task SaveItemLabelSingularAsync(string label, CancellationToken cancellationToken = default) => Task.CompletedTask;
            public Task<string> GetItemLabelPluralAsync(CancellationToken cancellationToken = default) => Task.FromResult("Devices");
            public Task SaveItemLabelPluralAsync(string label, CancellationToken cancellationToken = default) => Task.CompletedTask;
            public Task<IDictionary<ItemDetailField, bool>> GetItemDetailVisibilityAsync(CancellationToken cancellationToken = default)
                => Task.FromResult<IDictionary<ItemDetailField, bool>>(new Dictionary<ItemDetailField, bool>());
            public Task SaveItemDetailVisibilityAsync(IDictionary<ItemDetailField, bool> visibility, CancellationToken cancellationToken = default)
            { ItemDetailVisibilityChanged?.Invoke(this, visibility); return Task.CompletedTask; }
            private string? _theme;
            public Dictionary<string, string> Settings { get; } = new();
        }

        private sealed class DummyDialogService : IDialogService
        {
            public void ShowInfo(string message, string title) { }
            public bool ShowConfirmation(string message, string title) => true;
        }

        [Fact]
        public async Task Theme_Setter_SavesValue()
        {
            var service = new StubSettingsService();
            var vm = new SettingsViewModel(service, new DummyDialogService());
            await vm.InitializeAsync();
            vm.Theme = "Dark";
            Assert.Equal("Dark", service.ThemeSaved);
        }
    }
}
