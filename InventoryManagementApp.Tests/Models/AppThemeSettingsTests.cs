using InventoryManagementApp.Interfaces;
using InventoryManagementApp.Models;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace InventoryManagementApp.Tests.Models
{
    public class AppThemeSettingsTests
    {
        [Fact]
        public void Normalize_ClampsVisualControlsAndNormalizesColors()
        {
            var settings = new AppThemeSettings
            {
                BaseTheme = "Unexpected",
                BackgroundColor = "123456",
                SurfaceColor = "bad",
                BackgroundOpacity = 2,
                SurfaceOpacity = -1,
                ButtonCornerRadius = 99,
                ShadowDepth = -5,
                ShadowOpacity = 5,
                PagePadding = 100
            };

            settings.Normalize();

            Assert.Equal("Light", settings.BaseTheme);
            Assert.Equal("#123456", settings.BackgroundColor);
            Assert.Equal(AppThemeSettings.CreateDefault("Light").SurfaceColor, settings.SurfaceColor);
            Assert.Equal(1, settings.BackgroundOpacity);
            Assert.Equal(0, settings.SurfaceOpacity);
            Assert.Equal(32, settings.ButtonCornerRadius);
            Assert.Equal(0, settings.ShadowDepth);
            Assert.Equal(1, settings.ShadowOpacity);
            Assert.Equal(28, settings.PagePadding);
        }

        [Fact]
        public void CreateDefault_DarkUsesDarkPalette()
        {
            var settings = AppThemeSettings.CreateDefault("Dark");

            Assert.Equal("Dark", settings.BaseTheme);
            Assert.Equal("#FF101418", settings.BackgroundColor);
            Assert.Equal("#FFF3F4F6", settings.TextColor);
            Assert.True(settings.ShadowOpacity > AppThemeSettings.CreateDefault("Light").ShadowOpacity);
        }

        [Fact]
        public async Task SettingsServiceDefaults_SaveAndLoadThemeProfileAsSingleSetting()
        {
            ISettingsService service = new FakeSettingsService();
            var settings = AppThemeSettings.CreateDefault("Dark");
            settings.ButtonCornerRadius = 18;
            settings.BordersVisible = false;

            await service.SaveAppThemeSettingsAsync(settings);
            var loaded = await service.GetAppThemeSettingsAsync();

            Assert.Equal("Dark", loaded.BaseTheme);
            Assert.Equal(18, loaded.ButtonCornerRadius);
            Assert.False(loaded.BordersVisible);
        }

        private sealed class FakeSettingsService : ISettingsService
        {
            private readonly Dictionary<string, string> _settings = new();

            public event System.EventHandler<IDictionary<ItemDetailField, bool>>? ItemDetailVisibilityChanged;
            public event System.EventHandler<double>? ItemCardSizeChanged;

            public Task SaveSettingAsync(string key, string value, CancellationToken cancellationToken = default)
            {
                _settings[key] = value;
                return Task.CompletedTask;
            }

            public Task<string?> GetSettingAsync(string? key, CancellationToken cancellationToken = default)
            {
                return Task.FromResult(key != null && _settings.TryGetValue(key, out var value) ? value : null);
            }

            public Task<Dictionary<string, string>> GetAllSettingsAsync(CancellationToken cancellationToken = default) => Task.FromResult(new Dictionary<string, string>(_settings));
            public Task UpdateSettingsAsync(Dictionary<string, string> settings, CancellationToken cancellationToken = default)
            {
                foreach (var setting in settings)
                    _settings[setting.Key] = setting.Value;
                return Task.CompletedTask;
            }
            public Task DeleteSettingAsync(string key, CancellationToken cancellationToken = default)
            {
                _settings.Remove(key);
                return Task.CompletedTask;
            }
            public Task<string?> GetThemeAsync(CancellationToken cancellationToken = default) => GetSettingAsync("Theme", cancellationToken);
            public Task SaveThemeAsync(string theme, CancellationToken cancellationToken = default) => SaveSettingAsync("Theme", theme, cancellationToken);
            public Task<int> GetPasswordIterationsAsync(CancellationToken cancellationToken = default) => Task.FromResult(100000);
            public Task SavePasswordIterationsAsync(int iterations, CancellationToken cancellationToken = default) => Task.CompletedTask;
            public Task<int> GetAutoLogoutMinutesAsync(CancellationToken cancellationToken = default) => Task.FromResult(1);
            public Task SaveAutoLogoutMinutesAsync(int minutes, CancellationToken cancellationToken = default) => Task.CompletedTask;
            public Task<string> GetItemLabelSingularAsync(CancellationToken cancellationToken = default) => Task.FromResult("Item");
            public Task SaveItemLabelSingularAsync(string label, CancellationToken cancellationToken = default) => Task.CompletedTask;
            public Task<string> GetItemLabelPluralAsync(CancellationToken cancellationToken = default) => Task.FromResult("Items");
            public Task SaveItemLabelPluralAsync(string label, CancellationToken cancellationToken = default) => Task.CompletedTask;
            public Task<IDictionary<ItemDetailField, bool>> GetItemDetailVisibilityAsync(CancellationToken cancellationToken = default) => Task.FromResult<IDictionary<ItemDetailField, bool>>(new Dictionary<ItemDetailField, bool>());
            public Task SaveItemDetailVisibilityAsync(IDictionary<ItemDetailField, bool> visibility, CancellationToken cancellationToken = default)
            {
                ItemDetailVisibilityChanged?.Invoke(this, visibility);
                return Task.CompletedTask;
            }
            public Task<double> GetItemCardSizeAsync(CancellationToken cancellationToken = default) => Task.FromResult(1.0);
            public Task SaveItemCardSizeAsync(double size, CancellationToken cancellationToken = default)
            {
                ItemCardSizeChanged?.Invoke(this, size);
                return Task.CompletedTask;
            }
        }
    }
}
