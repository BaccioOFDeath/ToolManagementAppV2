using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Documents;
using InventoryManagementApp.Interfaces;
using InventoryManagementApp.Models;
using InventoryManagementApp.ViewModels;
using Xunit;

public class ThemeDesignerViewModelTests
{
    [Fact]
    public async Task SaveCommand_PersistsFullThemeProfileAndBaseTheme()
    {
        var settingsService = new FakeSettingsService();
        var themeService = new RecordingThemeService();
        var viewModel = CreateViewModel(settingsService, themeService);

        await viewModel.InitializeAsync();
        viewModel.BaseTheme = "Dark";
        viewModel.AccentColor = "60a5fa";
        viewModel.TransparentCanvasPresetCommand.Execute(null);

        await viewModel.SaveCommand.ExecuteAsync(null);

        Assert.Equal("Dark", settingsService.SavedTheme);
        var saved = JsonSerializer.Deserialize<AppThemeSettings>(settingsService.Settings[ISettingsService.AppThemeSettingsKey])!;
        Assert.Equal("Dark", saved.BaseTheme);
        Assert.False(saved.BordersVisible);
        Assert.False(saved.EnableSurfaceShadows);
        Assert.False(saved.EnableControlShadows);
        Assert.Equal(0.18, saved.SurfaceOpacity);
        Assert.Equal(0, saved.BorderThickness);
        Assert.Equal(0, saved.ControlBorderThickness);
        Assert.Equal("#60A5FA", saved.AccentColor);
        Assert.Equal("Theme saved and applied.", viewModel.Status);
        Assert.Contains(themeService.AppliedCustomThemes, theme => theme.BordersVisible == false && theme.SurfaceOpacity == 0.18);
    }

    [Fact]
    public async Task Initialize_LoadsSavedThemeProfileAndPreviewsIt()
    {
        var savedTheme = AppThemeSettings.CreateDefault("Dark");
        savedTheme.ButtonCornerRadius = 18;
        savedTheme.SurfaceOpacity = 0.52;
        savedTheme.EnableControlShadows = true;
        savedTheme.Normalize();

        var settingsService = new FakeSettingsService();
        settingsService.Settings[ISettingsService.AppThemeSettingsKey] = JsonSerializer.Serialize(savedTheme);
        var themeService = new RecordingThemeService();
        var viewModel = CreateViewModel(settingsService, themeService);

        await viewModel.InitializeAsync();

        Assert.Equal("Dark", viewModel.BaseTheme);
        Assert.Equal(18, viewModel.ButtonCornerRadius);
        Assert.Equal(0.52, viewModel.SurfaceOpacity);
        Assert.True(viewModel.EnableControlShadows);
        Assert.Equal("Loaded saved app theme.", viewModel.Status);
        Assert.Contains(themeService.AppliedCustomThemes, theme => theme.ButtonCornerRadius == 18 && theme.SurfaceOpacity == 0.52);
    }

    [Fact]
    public void PresetCommands_PreviewAndNotifyChangedDesignerControls()
    {
        var themeService = new RecordingThemeService();
        var viewModel = CreateViewModel(new FakeSettingsService(), themeService);
        var changed = new List<string>();
        viewModel.PropertyChanged += (_, e) =>
        {
            if (!string.IsNullOrWhiteSpace(e.PropertyName))
                changed.Add(e.PropertyName);
        };

        viewModel.DeepShadowPresetCommand.Execute(null);

        Assert.Equal("Deep shadow preset previewed. Save to keep it.", viewModel.Status);
        Assert.True(viewModel.EnableSurfaceShadows);
        Assert.True(viewModel.EnableControlShadows);
        Assert.Equal(36, viewModel.ShadowBlurRadius);
        Assert.Equal(12, viewModel.ShadowDepth);
        Assert.Equal(2.2, viewModel.SurfaceShadowScale);
        Assert.Contains(nameof(ThemeDesignerViewModel.ShadowDepth), changed);
        Assert.Contains(nameof(ThemeDesignerViewModel.SurfaceShadowScale), changed);
        Assert.Contains(nameof(ThemeDesignerViewModel.ControlShadowScale), changed);
        Assert.Contains(themeService.AppliedCustomThemes, theme => theme.ShadowDepth == 12 && theme.SurfaceShadowScale == 2.2);
    }

    private static ThemeDesignerViewModel CreateViewModel(FakeSettingsService settingsService, RecordingThemeService themeService)
        => new(settingsService, themeService, new FakeFileDialogService(), new FakeDialogService());

    private sealed class FakeSettingsService : ISettingsService
    {
        public event EventHandler<IDictionary<ItemDetailField, bool>>? ItemDetailVisibilityChanged;
        public event EventHandler<double>? ItemCardSizeChanged;

        public Dictionary<string, string> Settings { get; } = new();
        public string? SavedTheme { get; private set; } = "Light";

        public Task SaveSettingAsync(string key, string value, CancellationToken cancellationToken = default)
        {
            Settings[key] = value;
            return Task.CompletedTask;
        }

        public Task<string?> GetSettingAsync(string? key, CancellationToken cancellationToken = default)
        {
            if (key == null)
                return Task.FromResult<string?>(null);

            return Task.FromResult(Settings.TryGetValue(key, out var value) ? value : null);
        }

        public Task<Dictionary<string, string>> GetAllSettingsAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(new Dictionary<string, string>(Settings));

        public Task UpdateSettingsAsync(Dictionary<string, string> settings, CancellationToken cancellationToken = default)
        {
            foreach (var pair in settings)
                Settings[pair.Key] = pair.Value;
            return Task.CompletedTask;
        }

        public Task DeleteSettingAsync(string key, CancellationToken cancellationToken = default)
        {
            Settings.Remove(key);
            return Task.CompletedTask;
        }

        public Task<string?> GetThemeAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(SavedTheme);

        public Task SaveThemeAsync(string theme, CancellationToken cancellationToken = default)
        {
            SavedTheme = theme;
            return Task.CompletedTask;
        }

        public Task<int> GetPasswordIterationsAsync(CancellationToken cancellationToken = default) => Task.FromResult(100_000);
        public Task SavePasswordIterationsAsync(int iterations, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task<int> GetAutoLogoutMinutesAsync(CancellationToken cancellationToken = default) => Task.FromResult(15);
        public Task SaveAutoLogoutMinutesAsync(int minutes, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task<string> GetItemLabelSingularAsync(CancellationToken cancellationToken = default) => Task.FromResult("Item");
        public Task SaveItemLabelSingularAsync(string label, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task<string> GetItemLabelPluralAsync(CancellationToken cancellationToken = default) => Task.FromResult("Items");
        public Task SaveItemLabelPluralAsync(string label, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task<IDictionary<ItemDetailField, bool>> GetItemDetailVisibilityAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IDictionary<ItemDetailField, bool>>(new Dictionary<ItemDetailField, bool>());
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

    private sealed class RecordingThemeService : IThemeService
    {
        public List<string?> AppliedBaseThemes { get; } = new();
        public List<AppThemeSettings> AppliedCustomThemes { get; } = new();

        public void ApplyTheme(string? theme) => AppliedBaseThemes.Add(theme);

        public void ApplyCustomTheme(AppThemeSettings? settings)
        {
            if (settings == null)
                return;

            AppliedCustomThemes.Add(JsonSerializer.Deserialize<AppThemeSettings>(JsonSerializer.Serialize(settings))!);
        }
    }

    private sealed class FakeFileDialogService : IFileDialogService
    {
        public string? OpenFile(string filter, string? initialDirectory = null) => null;
        public string? SaveFile(string filter, string? initialDirectory = null) => null;
        public string? BrowseFolder(string? initialDirectory = null) => null;
    }

    private sealed class FakeDialogService : IDialogService
    {
        public void ShowInfo(string message, string title) { }
        public bool ShowConfirmation(string message, string title) => true;
        public ItemModel? ShowEditItemDialog(ItemModel item) => item;
        public void ShowItemDetails(ItemModel item) { }
        public (CustomerModel customer, DateTime dueDate)? ShowRentItemDialog(ItemModel item, IEnumerable<CustomerModel> customers) => null;
        public CustomerModel? ShowAddCustomerDialog() => null;
        public CustomerModel? ShowEditCustomerDialog(CustomerModel customer) => customer;
        public void ShowRentalsFilter(ManageRentalsViewModel viewModel) { }
        public void ShowRentalHistory(ItemModel item, IEnumerable<RentalModel> history) { }
        public Dictionary<string, string>? ShowImportMapping(IEnumerable<string> headers, IEnumerable<string> properties, IEnumerable<string>? requiredPropertyNames = null) => null;
        public Func<ItemModel, IEnumerable<string>>? ShowImageImportMapping() => null;
        public void ShowPrintPreview(FlowDocument document, string title, string description) { }
        public void ShowPrintLabelDialog() { }
    }
}
