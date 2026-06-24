using InventoryManagementApp.Interfaces;
using InventoryManagementApp.Models;
using InventoryManagementApp.ViewModels;
using Moq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace InventoryManagementApp.Tests.ViewModels
{
    public class ThemeDesignerViewModelTests
    {
        [Fact]
        public async Task InitializeAsync_LoadsSavedThemeProfileAndAppliesPreview()
        {
            var settingsService = new FakeSettingsService();
            var saved = AppThemeSettings.CreateDefault("Dark");
            saved.BackgroundColor = "#FF010203";
            saved.SurfaceColor = "#FF111213";
            saved.ButtonCornerRadius = 17;
            saved.EnableControlShadows = true;
            saved.ControlShadowScale = 1.4;
            await ((ISettingsService)settingsService).SaveAppThemeSettingsAsync(saved);

            var themeService = new RecordingThemeService();
            var viewModel = CreateViewModel(settingsService, themeService);

            await viewModel.InitializeAsync();

            Assert.Equal("Dark", viewModel.BaseTheme);
            Assert.Equal("#FF010203", viewModel.BackgroundColor);
            Assert.Equal("#FF111213", viewModel.SurfaceColor);
            Assert.Equal(17, viewModel.ButtonCornerRadius);
            Assert.True(viewModel.EnableControlShadows);
            Assert.Equal(1.4, viewModel.ControlShadowScale);
            Assert.NotNull(themeService.LastCustomTheme);
            Assert.Equal("Dark", themeService.LastCustomTheme!.BaseTheme);
            Assert.Equal("Loaded saved app theme.", viewModel.Status);
        }

        [Fact]
        public async Task SaveCommand_NormalizesPersistsAndAppliesFullThemeProfile()
        {
            var settingsService = new FakeSettingsService();
            var themeService = new RecordingThemeService();
            var viewModel = CreateViewModel(settingsService, themeService);
            await viewModel.InitializeAsync();

            viewModel.BaseTheme = "Dark";
            viewModel.BackgroundColor = "112233";
            viewModel.ButtonColor = "445566";
            viewModel.BordersVisible = false;
            viewModel.ButtonCornerRadius = 21;
            viewModel.ShadowDepth = 8;
            viewModel.ControlShadowScale = 0.75;
            viewModel.BackgroundImageStretch = "Uniform";
            viewModel.DashboardHeaderColor = "123456";
            viewModel.RentalsHeaderColor = "654321";

            await viewModel.SaveCommand.ExecuteAsync(null);
            var loaded = await ((ISettingsService)settingsService).GetAppThemeSettingsAsync();

            Assert.Equal("Dark", await settingsService.GetThemeAsync());
            Assert.Equal("Dark", loaded.BaseTheme);
            Assert.Equal("#112233", loaded.BackgroundColor);
            Assert.Equal("#445566", loaded.ButtonColor);
            Assert.False(loaded.BordersVisible);
            Assert.Equal(21, loaded.ButtonCornerRadius);
            Assert.Equal(8, loaded.ShadowDepth);
            Assert.Equal(0.75, loaded.ControlShadowScale);
            Assert.Equal("Uniform", loaded.BackgroundImageStretch);
            Assert.Equal("#123456", loaded.DashboardHeaderColor);
            Assert.Equal("#654321", loaded.RentalsHeaderColor);
            Assert.Equal("Theme saved and applied.", viewModel.Status);
            Assert.NotNull(themeService.LastCustomTheme);
            Assert.Equal("#112233", themeService.LastCustomTheme!.BackgroundColor);
            Assert.Equal("#123456", themeService.LastCustomTheme.DashboardHeaderColor);
            Assert.True(themeService.CustomApplyCount > 1);
        }

        [Fact]
        public async Task BaseTheme_SelectionLoadsFullDefaultPaletteAndAppliesImmediately()
        {
            var settingsService = new FakeSettingsService();
            var themeService = new RecordingThemeService();
            var viewModel = CreateViewModel(settingsService, themeService);
            await viewModel.InitializeAsync();

            viewModel.AccentColor = "#FFFF0000";
            viewModel.SelectedColor = "#FFFF0000";

            viewModel.BaseTheme = "Dark";

            var darkDefaults = AppThemeSettings.CreateDefault("Dark");
            Assert.Equal("Dark", viewModel.BaseTheme);
            Assert.Equal(darkDefaults.BackgroundColor, viewModel.BackgroundColor);
            Assert.Equal(darkDefaults.SurfaceColor, viewModel.SurfaceColor);
            Assert.Equal(darkDefaults.AccentColor, viewModel.AccentColor);
            Assert.Equal(darkDefaults.SelectedColor, viewModel.SelectedColor);
            Assert.NotNull(themeService.LastCustomTheme);
            Assert.Equal("Dark", themeService.LastCustomTheme!.BaseTheme);
            Assert.Equal(darkDefaults.SelectedColor, themeService.LastCustomTheme.SelectedColor);
            Assert.Equal("Dark theme previewed. Save to keep it.", viewModel.Status);
        }

        [Fact]
        public async Task ThemeOptions_ExposeVSCodeBaseTheme()
        {
            var settingsService = new FakeSettingsService();
            var themeService = new RecordingThemeService();
            var viewModel = CreateViewModel(settingsService, themeService);
            await viewModel.InitializeAsync();

            Assert.Contains("VS Code", viewModel.ThemeOptions);
            Assert.Contains("VS Code Light", viewModel.ThemeOptions);

            viewModel.BaseTheme = "VS Code";

            var defaults = AppThemeSettings.CreateDefault("VS Code");
            Assert.Equal("VS Code", viewModel.BaseTheme);
            Assert.Equal(defaults.BackgroundColor, viewModel.BackgroundColor);
            Assert.Equal(defaults.NavigationColor, viewModel.NavigationColor);
            Assert.Equal(defaults.SelectedColor, viewModel.SelectedColor);
            Assert.False(viewModel.EnableSurfaceShadows);
            Assert.NotNull(themeService.LastCustomTheme);
            Assert.Equal("VS Code", themeService.LastCustomTheme!.BaseTheme);
            Assert.Equal("VS Code theme previewed. Save to keep it.", viewModel.Status);
        }

        [Fact]
        public async Task ThemeOptions_ExposeVSCodeLightBaseTheme()
        {
            var settingsService = new FakeSettingsService();
            var themeService = new RecordingThemeService();
            var viewModel = CreateViewModel(settingsService, themeService);
            await viewModel.InitializeAsync();

            viewModel.BaseTheme = "VS Code Light";

            var defaults = AppThemeSettings.CreateDefault("VS Code Light");
            Assert.Equal("VS Code Light", viewModel.BaseTheme);
            Assert.Equal(defaults.BackgroundColor, viewModel.BackgroundColor);
            Assert.Equal(defaults.NavigationColor, viewModel.NavigationColor);
            Assert.Equal(defaults.SelectedColor, viewModel.SelectedColor);
            Assert.False(viewModel.EnableSurfaceShadows);
            Assert.NotNull(themeService.LastCustomTheme);
            Assert.Equal("VS Code Light", themeService.LastCustomTheme!.BaseTheme);
            Assert.Equal("VS Code Light theme previewed. Save to keep it.", viewModel.Status);
        }

        [Fact]
        public void AppThemeSettings_NormalizeDefaultsPageHeadersToSurfaceColor()
        {
            var settings = AppThemeSettings.CreateDefault("Dark");
            settings.DashboardHeaderColor = string.Empty;
            settings.RentalsHeaderColor = string.Empty;

            settings.Normalize();

            Assert.Equal(settings.SurfaceColor, settings.DashboardHeaderColor);
            Assert.Equal(settings.SurfaceColor, settings.RentalsHeaderColor);
        }

        [Fact]
        public async Task TransparentCanvasPreset_PreviewsBackgroundFirstRedesign()
        {
            var settingsService = new FakeSettingsService();
            var themeService = new RecordingThemeService();
            var viewModel = CreateViewModel(settingsService, themeService);
            await viewModel.InitializeAsync();

            viewModel.TransparentCanvasPresetCommand.Execute(null);

            Assert.True(viewModel.UseGlassSurfaces);
            Assert.False(viewModel.BordersVisible);
            Assert.False(viewModel.EnableSurfaceShadows);
            Assert.False(viewModel.EnableControlShadows);
            Assert.Equal(0, viewModel.BackgroundOverlayOpacity);
            Assert.Equal(0.18, viewModel.SurfaceOpacity);
            Assert.Equal(0.12, viewModel.SurfaceAltOpacity);
            Assert.Equal(0.24, viewModel.InputOpacity);
            Assert.Equal(0.22, viewModel.ButtonOpacity);
            Assert.Equal(0.18, viewModel.NavigationOpacity);
            Assert.Equal(0.16, viewModel.HeaderOpacity);
            Assert.Equal(0.82, viewModel.MenuDropDownOpacity);
            Assert.Equal(0, viewModel.BorderThickness);
            Assert.Equal(0, viewModel.ControlBorderThickness);
            Assert.Equal(0, viewModel.ShadowDepth);
            Assert.Equal(0.06, viewModel.GridLineOpacity);
            Assert.NotNull(themeService.LastCustomTheme);
            Assert.Equal(0.18, themeService.LastCustomTheme!.SurfaceOpacity);
            Assert.Equal("Transparent canvas preset previewed. Save to keep it.", viewModel.Status);
        }

        [Fact]
        public async Task BorderlessPreset_PreviewsCompleteBorderAndShadowRemoval()
        {
            var settingsService = new FakeSettingsService();
            var themeService = new RecordingThemeService();
            var viewModel = CreateViewModel(settingsService, themeService);
            await viewModel.InitializeAsync();

            viewModel.BorderlessPresetCommand.Execute(null);

            Assert.False(viewModel.BordersVisible);
            Assert.False(viewModel.EnableSurfaceShadows);
            Assert.False(viewModel.EnableControlShadows);
            Assert.Equal(0, viewModel.BorderOpacity);
            Assert.Equal(0, viewModel.BorderThickness);
            Assert.Equal(0, viewModel.ControlBorderThickness);
            Assert.Equal(0, viewModel.ShadowBlurRadius);
            Assert.Equal(0, viewModel.ShadowDepth);
            Assert.Equal(0, viewModel.ShadowOpacity);
            Assert.Equal(0, viewModel.SurfaceShadowScale);
            Assert.Equal(0, viewModel.ControlShadowScale);
            Assert.NotNull(themeService.LastCustomTheme);
            Assert.False(themeService.LastCustomTheme!.BordersVisible);
            Assert.Equal("Borderless preset previewed. Save to keep it.", viewModel.Status);
        }

        [Fact]
        public async Task DeepShadowPreset_PreviewsRaisedSurfaceAndControlDepth()
        {
            var settingsService = new FakeSettingsService();
            var themeService = new RecordingThemeService();
            var viewModel = CreateViewModel(settingsService, themeService);
            await viewModel.InitializeAsync();

            viewModel.DeepShadowPresetCommand.Execute(null);

            Assert.True(viewModel.BordersVisible);
            Assert.True(viewModel.EnableSurfaceShadows);
            Assert.True(viewModel.EnableControlShadows);
            Assert.Equal(36, viewModel.ShadowBlurRadius);
            Assert.Equal(12, viewModel.ShadowDepth);
            Assert.Equal(0.45, viewModel.ShadowOpacity);
            Assert.Equal(2.2, viewModel.SurfaceShadowScale);
            Assert.Equal(1.6, viewModel.ControlShadowScale);
            Assert.Equal(12, viewModel.CardCornerRadius);
            Assert.Equal(8, viewModel.ButtonCornerRadius);
            Assert.Equal(1.25, viewModel.InteractionIntensity);
            Assert.NotNull(themeService.LastCustomTheme);
            Assert.True(themeService.LastCustomTheme!.EnableControlShadows);
            Assert.Equal(1.6, themeService.LastCustomTheme.ControlShadowScale);
            Assert.Equal("Deep shadow preset previewed. Save to keep it.", viewModel.Status);
        }

        private static ThemeDesignerViewModel CreateViewModel(FakeSettingsService settingsService, RecordingThemeService themeService)
        {
            return new ThemeDesignerViewModel(
                settingsService,
                themeService,
                new Mock<IFileDialogService>().Object,
                new Mock<IDialogService>().Object);
        }

        private sealed class RecordingThemeService : IThemeService
        {
            public AppThemeSettings? LastCustomTheme { get; private set; }
            public int CustomApplyCount { get; private set; }

            public void ApplyTheme(string? theme)
            {
            }

            public void ApplyCustomTheme(AppThemeSettings? settings)
            {
                if (settings == null)
                {
                    LastCustomTheme = null;
                    return;
                }

                var copy = new AppThemeSettings
                {
                    BaseTheme = settings.BaseTheme,
                    BackgroundColor = settings.BackgroundColor,
                    BackgroundOverlayColor = settings.BackgroundOverlayColor,
                    SurfaceColor = settings.SurfaceColor,
                    SurfaceAltColor = settings.SurfaceAltColor,
                    NavigationColor = settings.NavigationColor,
                    InputColor = settings.InputColor,
                    ButtonColor = settings.ButtonColor,
                    BorderColor = settings.BorderColor,
                    TextColor = settings.TextColor,
                    MutedTextColor = settings.MutedTextColor,
                    AccentColor = settings.AccentColor,
                    HoverColor = settings.HoverColor,
                    HoverTextColor = settings.HoverTextColor,
                    SelectedColor = settings.SelectedColor,
                    SelectedTextColor = settings.SelectedTextColor,
                    SuccessColor = settings.SuccessColor,
                    WarningColor = settings.WarningColor,
                    ErrorColor = settings.ErrorColor,
                    ShadowColor = settings.ShadowColor,
                    DashboardHeaderColor = settings.DashboardHeaderColor,
                    SearchHeaderColor = settings.SearchHeaderColor,
                    ManageItemsHeaderColor = settings.ManageItemsHeaderColor,
                    RentalsHeaderColor = settings.RentalsHeaderColor,
                    CustomersHeaderColor = settings.CustomersHeaderColor,
                    ReservationsHeaderColor = settings.ReservationsHeaderColor,
                    MaintenanceHeaderColor = settings.MaintenanceHeaderColor,
                    CalibrationHeaderColor = settings.CalibrationHeaderColor,
                    KitsHeaderColor = settings.KitsHeaderColor,
                    CategoriesHeaderColor = settings.CategoriesHeaderColor,
                    ReportsHeaderColor = settings.ReportsHeaderColor,
                    ActivityLogsHeaderColor = settings.ActivityLogsHeaderColor,
                    ImportExportHeaderColor = settings.ImportExportHeaderColor,
                    UsersHeaderColor = settings.UsersHeaderColor,
                    SettingsHeaderColor = settings.SettingsHeaderColor,
                    BackgroundImagePath = settings.BackgroundImagePath,
                    BackgroundImageStretch = settings.BackgroundImageStretch,
                    FontFamily = settings.FontFamily,
                    BackgroundOpacity = settings.BackgroundOpacity,
                    BackgroundOverlayOpacity = settings.BackgroundOverlayOpacity,
                    SurfaceOpacity = settings.SurfaceOpacity,
                    SurfaceAltOpacity = settings.SurfaceAltOpacity,
                    InputOpacity = settings.InputOpacity,
                    ButtonOpacity = settings.ButtonOpacity,
                    NavigationOpacity = settings.NavigationOpacity,
                    HeaderOpacity = settings.HeaderOpacity,
                    MenuOpacity = settings.MenuOpacity,
                    MenuDropDownOpacity = settings.MenuDropDownOpacity,
                    FooterOpacity = settings.FooterOpacity,
                    DialogOpacity = settings.DialogOpacity,
                    DisabledOpacity = settings.DisabledOpacity,
                    BordersVisible = settings.BordersVisible,
                    BorderOpacity = settings.BorderOpacity,
                    BorderThickness = settings.BorderThickness,
                    ControlBorderThickness = settings.ControlBorderThickness,
                    DividerOpacity = settings.DividerOpacity,
                    CardCornerRadius = settings.CardCornerRadius,
                    PanelCornerRadius = settings.PanelCornerRadius,
                    ButtonCornerRadius = settings.ButtonCornerRadius,
                    InputCornerRadius = settings.InputCornerRadius,
                    ShadowBlurRadius = settings.ShadowBlurRadius,
                    ShadowDepth = settings.ShadowDepth,
                    ShadowOpacity = settings.ShadowOpacity,
                    ShadowDirection = settings.ShadowDirection,
                    SurfaceShadowScale = settings.SurfaceShadowScale,
                    ControlShadowScale = settings.ControlShadowScale,
                    PagePadding = settings.PagePadding,
                    CardPadding = settings.CardPadding,
                    FontScale = settings.FontScale,
                    HeadingFontScale = settings.HeadingFontScale,
                    ControlHeight = settings.ControlHeight,
                    DataGridRowHeight = settings.DataGridRowHeight,
                    DataGridHeaderHeight = settings.DataGridHeaderHeight,
                    InteractionIntensity = settings.InteractionIntensity,
                    FocusRingOpacity = settings.FocusRingOpacity,
                    GridLineOpacity = settings.GridLineOpacity,
                    MotionIntensity = settings.MotionIntensity,
                    UseGlassSurfaces = settings.UseGlassSurfaces,
                    EnableSurfaceShadows = settings.EnableSurfaceShadows,
                    EnableControlShadows = settings.EnableControlShadows
                };
                copy.Normalize();
                LastCustomTheme = copy;
                CustomApplyCount++;
            }
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
