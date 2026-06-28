using InventoryManagementApp.Interfaces;
using InventoryManagementApp.Models;
using System;
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
                BackgroundOverlayColor = "#GGGGGG",
                SurfaceColor = "bad",
                NavigationColor = "445566",
                InputColor = "not-a-color",
                ButtonColor = "778899",
                BorderColor = "abcdef",
                HoverColor = "121212",
                HoverTextColor = "fefefe",
                SelectedColor = "13579b",
                SelectedTextColor = "ffffff",
                ShadowColor = "112233",
                BackgroundImageStretch = "Tile",
                FontFamily = "  Aptos  ",
                BackgroundOpacity = 2,
                BackgroundOverlayOpacity = 3,
                SurfaceOpacity = -1,
                DisabledOpacity = 0,
                BorderThickness = 99,
                ControlBorderThickness = -2,
                DividerOpacity = 4,
                ButtonCornerRadius = 99,
                ShadowDepth = -5,
                ShadowOpacity = 5,
                ShadowDirection = 999,
                SurfaceShadowScale = 8,
                ControlShadowScale = double.NaN,
                PagePadding = 100,
                NavigationOpacity = 4,
                MenuDropDownOpacity = -2,
                FontScale = 3,
                HeadingFontScale = 4,
                ControlHeight = 99,
                DataGridRowHeight = 2,
                DataGridHeaderHeight = 99,
                InteractionIntensity = 5,
                FocusRingOpacity = -2,
                GridLineOpacity = 4,
                MotionIntensity = double.PositiveInfinity
            };

            settings.Normalize();

            Assert.Equal("Light", settings.BaseTheme);
            Assert.Equal("#123456", settings.BackgroundColor);
            Assert.Equal(AppThemeSettings.CreateDefault("Light").BackgroundOverlayColor, settings.BackgroundOverlayColor);
            Assert.Equal(AppThemeSettings.CreateDefault("Light").SurfaceColor, settings.SurfaceColor);
            Assert.Equal("#445566", settings.NavigationColor);
            Assert.Equal(AppThemeSettings.CreateDefault("Light").InputColor, settings.InputColor);
            Assert.Equal("#778899", settings.ButtonColor);
            Assert.Equal("#ABCDEF", settings.BorderColor);
            Assert.Equal("#121212", settings.HoverColor);
            Assert.Equal("#FEFEFE", settings.HoverTextColor);
            Assert.Equal("#13579B", settings.SelectedColor);
            Assert.Equal("#FFFFFF", settings.SelectedTextColor);
            Assert.Equal("#112233", settings.ShadowColor);
            Assert.Equal("UniformToFill", settings.BackgroundImageStretch);
            Assert.Equal("Aptos", settings.FontFamily);
            Assert.Equal(1, settings.BackgroundOpacity);
            Assert.Equal(1, settings.BackgroundOverlayOpacity);
            Assert.Equal(0, settings.SurfaceOpacity);
            Assert.Equal(0.15, settings.DisabledOpacity);
            Assert.Equal(6, settings.BorderThickness);
            Assert.Equal(0, settings.ControlBorderThickness);
            Assert.Equal(1, settings.DividerOpacity);
            Assert.Equal(32, settings.ButtonCornerRadius);
            Assert.Equal(0, settings.ShadowDepth);
            Assert.Equal(1, settings.ShadowOpacity);
            Assert.Equal(360, settings.ShadowDirection);
            Assert.Equal(3, settings.SurfaceShadowScale);
            Assert.Equal(0, settings.ControlShadowScale);
            Assert.Equal(28, settings.PagePadding);
            Assert.Equal(1, settings.NavigationOpacity);
            Assert.Equal(0, settings.MenuDropDownOpacity);
            Assert.Equal(1.4, settings.FontScale);
            Assert.Equal(1.6, settings.HeadingFontScale);
            Assert.Equal(44, settings.ControlHeight);
            Assert.Equal(22, settings.DataGridRowHeight);
            Assert.Equal(56, settings.DataGridHeaderHeight);
            Assert.Equal(2, settings.InteractionIntensity);
            Assert.Equal(0, settings.FocusRingOpacity);
            Assert.Equal(1, settings.GridLineOpacity);
            Assert.Equal(0, settings.MotionIntensity);
        }

        [Fact]
        public void Normalize_AcceptsComboBoxItemTextForDarkTheme()
        {
            var settings = AppThemeSettings.CreateDefault("System.Windows.Controls.ComboBoxItem: Dark");

            Assert.Equal("Dark", settings.BaseTheme);
            Assert.Equal("#FF101418", settings.BackgroundColor);
            Assert.Equal("#CC101418", settings.BackgroundOverlayColor);
            Assert.Equal("#FF252D36", settings.NavigationColor);
            Assert.Equal("#FF1B222A", settings.InputColor);
            Assert.Equal("#FF252D36", settings.ButtonColor);
            Assert.Equal("#FF60A5FA", settings.BorderColor);
            Assert.Equal("#FF1E3A5F", settings.HoverColor);
            Assert.Equal("#FFF3F4F6", settings.HoverTextColor);
            Assert.Equal("#FF2563EB", settings.SelectedColor);
            Assert.Equal("#FFFFFFFF", settings.SelectedTextColor);
        }

        [Fact]
        public void CreateDefault_BuildsVSCodeThemePalette()
        {
            var settings = AppThemeSettings.CreateDefault("vscode");

            Assert.Equal("VS Code", settings.BaseTheme);
            Assert.Equal("#FF1E1E1E", settings.BackgroundColor);
            Assert.Equal("#FF181818", settings.NavigationColor);
            Assert.Equal("#FF252526", settings.SurfaceAltColor);
            Assert.Equal("#FF313131", settings.InputColor);
            Assert.Equal("#FF0E639C", settings.ButtonColor);
            Assert.Equal("#FF007ACC", settings.AccentColor);
            Assert.Equal("#FF2A2D2E", settings.HoverColor);
            Assert.Equal("#FF04395E", settings.SelectedColor);
            Assert.Equal("#FFCCCCCC", settings.TextColor);
            Assert.Equal("#FF858585", settings.MutedTextColor);
            Assert.False(settings.EnableSurfaceShadows);
            Assert.False(settings.EnableControlShadows);
            Assert.Equal(0, settings.CardCornerRadius);
            Assert.Equal(2, settings.InputCornerRadius);
            Assert.Equal(26, settings.ControlHeight);
        }

        [Fact]
        public void CreateDefault_BuildsVSCodeLightThemePalette()
        {
            var settings = AppThemeSettings.CreateDefault("VS Code Light");

            Assert.Equal("VS Code Light", settings.BaseTheme);
            Assert.Equal("#FFF3F3F3", settings.BackgroundColor);
            Assert.Equal("#FFF3F3F3", settings.NavigationColor);
            Assert.Equal("#FFF8F8F8", settings.SurfaceAltColor);
            Assert.Equal("#FFFFFFFF", settings.InputColor);
            Assert.Equal("#FFE5E5E5", settings.ButtonColor);
            Assert.Equal("#FF007ACC", settings.AccentColor);
            Assert.Equal("#FFE8E8E8", settings.HoverColor);
            Assert.Equal("#FFADD6FF", settings.SelectedColor);
            Assert.Equal("#FF333333", settings.TextColor);
            Assert.Equal("#FF6A6A6A", settings.MutedTextColor);
            Assert.False(settings.EnableSurfaceShadows);
            Assert.False(settings.EnableControlShadows);
            Assert.Equal(0, settings.CardCornerRadius);
            Assert.Equal(2, settings.InputCornerRadius);
            Assert.Equal(26, settings.ControlHeight);
        }

        [Theory]
        [InlineData("SD European Light", "#FFF7F8FA", "#FFF5B700", "#FFFFFFFF")]
        [InlineData("SD European Dark", "#FF0F0F0F", "#FFF5B700", "#FF1C1C1E")]
        public void CreateDefault_BuildsSDEuropeanThemePalettes(string theme, string background, string button, string surface)
        {
            var settings = AppThemeSettings.CreateDefault(theme);

            Assert.Equal(theme, settings.BaseTheme);
            Assert.Equal(background, settings.BackgroundColor);
            Assert.Equal(surface, settings.SurfaceColor);
            Assert.Equal("#FFF5B700", settings.AccentColor);
            Assert.Equal(button, settings.ButtonColor);
            Assert.Equal("#FF0F0F0F", settings.SelectedTextColor);
            Assert.Equal(12, settings.CardCornerRadius);
            Assert.Equal(20, settings.ButtonCornerRadius);
            Assert.Equal(32, settings.ControlHeight);
        }

        [Theory]
        [InlineData("None")]
        [InlineData("Fill")]
        [InlineData("Uniform")]
        [InlineData("UniformToFill")]
        public void Normalize_PreservesSupportedBackgroundStretchModes(string stretch)
        {
            var settings = new AppThemeSettings { BackgroundImageStretch = stretch };

            settings.Normalize();

            Assert.Equal(stretch, settings.BackgroundImageStretch);
        }

        [Fact]
        public async Task SettingsServiceDefaults_SaveAndLoadThemeProfileAsSingleSetting()
        {
            ISettingsService service = new FakeSettingsService();
            var settings = AppThemeSettings.CreateDefault("Dark");
            settings.BackgroundOverlayColor = "#AA223344";
            settings.BackgroundOverlayOpacity = 0.28;
            settings.MenuDropDownOpacity = 0.83;
            settings.NavigationColor = "#FF111111";
            settings.InputColor = "#FF222222";
            settings.ButtonColor = "#FF333333";
            settings.BorderColor = "#FF444444";
            settings.HoverColor = "#FF555555";
            settings.HoverTextColor = "#FFEEEEEE";
            settings.SelectedColor = "#FF666666";
            settings.SelectedTextColor = "#FFFFFFFF";
            settings.ShadowColor = "#AA000000";
            settings.FontFamily = "Aptos";
            settings.ButtonCornerRadius = 18;
            settings.BordersVisible = false;
            settings.BorderThickness = 2.5;
            settings.ControlBorderThickness = 1.5;
            settings.DividerOpacity = 0.36;
            settings.DisabledOpacity = 0.34;
            settings.FontScale = 1.15;
            settings.HeadingFontScale = 1.2;
            settings.DataGridRowHeight = 38;
            settings.BackgroundImageStretch = "Uniform";
            settings.InteractionIntensity = 1.6;
            settings.FocusRingOpacity = 0.75;
            settings.GridLineOpacity = 0.2;
            settings.MotionIntensity = 0.35;
            settings.ShadowDirection = 225;
            settings.SurfaceShadowScale = 1.7;
            settings.ControlShadowScale = 0.65;

            await service.SaveAppThemeSettingsAsync(settings);
            var loaded = await service.GetAppThemeSettingsAsync();

            Assert.Equal("Dark", loaded.BaseTheme);
            Assert.Equal("#AA223344", loaded.BackgroundOverlayColor);
            Assert.Equal(0.28, loaded.BackgroundOverlayOpacity);
            Assert.Equal(0.83, loaded.MenuDropDownOpacity);
            Assert.Equal("#FF111111", loaded.NavigationColor);
            Assert.Equal("#FF222222", loaded.InputColor);
            Assert.Equal("#FF333333", loaded.ButtonColor);
            Assert.Equal("#FF444444", loaded.BorderColor);
            Assert.Equal("#FF555555", loaded.HoverColor);
            Assert.Equal("#FFEEEEEE", loaded.HoverTextColor);
            Assert.Equal("#FF666666", loaded.SelectedColor);
            Assert.Equal("#FFFFFFFF", loaded.SelectedTextColor);
            Assert.Equal("#AA000000", loaded.ShadowColor);
            Assert.Equal("Aptos", loaded.FontFamily);
            Assert.Equal(18, loaded.ButtonCornerRadius);
            Assert.False(loaded.BordersVisible);
            Assert.Equal(2.5, loaded.BorderThickness);
            Assert.Equal(1.5, loaded.ControlBorderThickness);
            Assert.Equal(0.36, loaded.DividerOpacity);
            Assert.Equal(0.34, loaded.DisabledOpacity);
            Assert.Equal(1.15, loaded.FontScale);
            Assert.Equal(1.2, loaded.HeadingFontScale);
            Assert.Equal(38, loaded.DataGridRowHeight);
            Assert.Equal("Uniform", loaded.BackgroundImageStretch);
            Assert.Equal(1.6, loaded.InteractionIntensity);
            Assert.Equal(0.75, loaded.FocusRingOpacity);
            Assert.Equal(0.2, loaded.GridLineOpacity);
            Assert.Equal(0.35, loaded.MotionIntensity);
            Assert.Equal(225, loaded.ShadowDirection);
            Assert.Equal(1.7, loaded.SurfaceShadowScale);
            Assert.Equal(0.65, loaded.ControlShadowScale);
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
