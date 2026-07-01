using InventoryManagementApp.Interfaces;
using InventoryManagementApp.Models;
using InventoryManagementApp.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Documents;
using Xunit;

namespace InventoryManagementApp.Tests.ViewModels
{
    public class ThemeDesignerProfileTests
    {
        [Fact]
        public void ExportThemeProfileCommand_WritesNormalizedThemeJson()
        {
            var exportPath = Path.Combine(Path.GetTempPath(), $"inventory-theme-{Guid.NewGuid():N}.json");
            var backgroundPath = CreateTempImage(".png");
            var fileDialogs = new FakeFileDialogService { SavePath = exportPath };
            var viewModel = CreateViewModel(fileDialogs: fileDialogs);

            viewModel.BackgroundColor = "445566";
            viewModel.BackgroundImagePath = backgroundPath;
            viewModel.BorderThickness = 99;
            viewModel.ExportThemeProfileCommand.Execute(null);

            var exported = JsonSerializer.Deserialize<AppThemeSettings>(File.ReadAllText(exportPath));

            Assert.NotNull(exported);
            Assert.Equal("#445566", exported!.BackgroundColor);
            Assert.StartsWith(Path.Combine("Assets", "Backgrounds"), exported.BackgroundImagePath, StringComparison.OrdinalIgnoreCase);
            Assert.Equal(Path.GetFileName(exported.BackgroundImagePath), exported.BackgroundImageFileName);
            Assert.False(string.IsNullOrWhiteSpace(exported.BackgroundImageContentBase64));
            Assert.True(File.Exists(Path.Combine(AppContext.BaseDirectory, exported.BackgroundImagePath)));
            Assert.Equal(6, exported.BorderThickness);
            Assert.Equal("Theme profile exported.", viewModel.Status);
            Assert.Equal("Theme Profile (*.json)|*.json|All Files (*.*)|*.*", fileDialogs.LastSaveFilter);
        }

        [Fact]
        public void ImportThemeProfileCommand_PreviewsNormalizedProfileWithoutSaving()
        {
            var importPath = Path.Combine(Path.GetTempPath(), $"inventory-theme-{Guid.NewGuid():N}.json");
            File.WriteAllText(importPath, JsonSerializer.Serialize(new AppThemeSettings
            {
                BaseTheme = "Dark",
                BackgroundColor = "112233",
                ButtonCornerRadius = 99,
                SurfaceOpacity = -4,
                ShadowDepth = 12,
                FontFamily = "  Aptos  "
            }));

            var fileDialogs = new FakeFileDialogService { OpenPath = importPath };
            var settingsService = new FakeSettingsService();
            var themeService = new FakeThemeService();
            var viewModel = CreateViewModel(settingsService, themeService, fileDialogs);

            viewModel.ImportThemeProfileCommand.Execute(null);

            Assert.Equal("#112233", viewModel.BackgroundColor);
            Assert.Equal(32, viewModel.ButtonCornerRadius);
            Assert.Equal(0, viewModel.SurfaceOpacity);
            Assert.Equal(12, viewModel.ShadowDepth);
            Assert.Equal("Aptos", viewModel.FontFamily);
            Assert.Equal("Theme profile imported for preview. Save to keep it.", viewModel.Status);
            Assert.Equal(0, settingsService.SaveThemeCalls);
            Assert.Equal("Dark", themeService.LastAppliedCustomTheme?.BaseTheme);
            Assert.Equal("Theme Profile (*.json)|*.json|All Files (*.*)|*.*", fileDialogs.LastOpenFilter);
        }

        [Fact]
        public async Task SaveCommand_CopiesBackgroundToAppAssetsAndPersistsRelativePath()
        {
            var backgroundPath = CreateTempImage(".jpg");
            var settingsService = new FakeSettingsService();
            var viewModel = CreateViewModel(settingsService);
            viewModel.BackgroundImagePath = backgroundPath;

            await viewModel.SaveCommand.ExecuteAsync(null);
            var saved = await ((ISettingsService)settingsService).GetAppThemeSettingsAsync();

            Assert.StartsWith(Path.Combine("Assets", "Backgrounds"), saved.BackgroundImagePath, StringComparison.OrdinalIgnoreCase);
            Assert.True(File.Exists(Path.Combine(AppContext.BaseDirectory, saved.BackgroundImagePath)));
            Assert.Null(saved.BackgroundImageFileName);
            Assert.Null(saved.BackgroundImageContentBase64);
        }

        [Fact]
        public void ImportThemeProfileCommand_ExtractsEmbeddedBackgroundToAppAssets()
        {
            var importPath = Path.Combine(Path.GetTempPath(), $"inventory-theme-{Guid.NewGuid():N}.json");
            File.WriteAllText(importPath, JsonSerializer.Serialize(new AppThemeSettings
            {
                BackgroundImagePath = @"C:\OldMachine\Pictures\shop.png",
                BackgroundImageFileName = "shop.png",
                BackgroundImageContentBase64 = Convert.ToBase64String(new byte[] { 9, 8, 7, 6 })
            }));

            var fileDialogs = new FakeFileDialogService { OpenPath = importPath };
            var viewModel = CreateViewModel(fileDialogs: fileDialogs);

            viewModel.ImportThemeProfileCommand.Execute(null);

            Assert.StartsWith(Path.Combine("Assets", "Backgrounds"), viewModel.BackgroundImagePath, StringComparison.OrdinalIgnoreCase);
            Assert.True(File.Exists(Path.Combine(AppContext.BaseDirectory, viewModel.BackgroundImagePath)));
            Assert.Equal("Theme profile imported for preview. Save to keep it.", viewModel.Status);
        }

        [Fact]
        public void ThemeDesignerControl_ExposesThemeProfileImportExportActions()
        {
            var xaml = ReadRepositoryFile("InventoryManagementApp", "Views", "Pages", "ThemeDesignerControl.xaml");

            Assert.Contains("ImportThemeProfileCommand", xaml, StringComparison.Ordinal);
            Assert.Contains("ExportThemeProfileCommand", xaml, StringComparison.Ordinal);
            Assert.Contains("Theme profile backup", xaml, StringComparison.Ordinal);
            Assert.Contains("Portable JSON backups", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void ThemeDesignerControl_ExposesPresetToolbarWithoutHorizontalClipping()
        {
            var xaml = ReadRepositoryFile("InventoryManagementApp", "Views", "Pages", "ThemeDesignerControl.xaml");

            Assert.Contains("<WrapPanel HorizontalAlignment=\"Left\" Margin=\"0,10,0,0\">", xaml, StringComparison.Ordinal);
            Assert.Contains("TransparentCanvasPresetCommand", xaml, StringComparison.Ordinal);
            Assert.Contains("DeepShadowPresetCommand", xaml, StringComparison.Ordinal);
            Assert.Contains("Transparent Canvas", xaml, StringComparison.Ordinal);
            Assert.Contains("Deep Shadow", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void ThemeDesignerControl_ExposesColorPickersForHexThemeColors()
        {
            var xaml = ReadRepositoryFile("InventoryManagementApp", "Views", "Pages", "ThemeDesignerControl.xaml");

            Assert.Contains("xmlns:xctk=\"http://schemas.xceed.com/wpf/xaml/toolkit\"", xaml, StringComparison.Ordinal);
            Assert.Contains("xctk:ColorPicker", xaml, StringComparison.Ordinal);
            Assert.Contains("HexColorConverter", xaml, StringComparison.Ordinal);
            Assert.Contains("SelectedColor=\"{Binding BackgroundColor", xaml, StringComparison.Ordinal);
            Assert.Contains("SelectedColor=\"{Binding AccentColor", xaml, StringComparison.Ordinal);
            Assert.Contains("SelectedColor=\"{Binding SearchBarBackgroundColor", xaml, StringComparison.Ordinal);
            Assert.Contains("SelectedColor=\"{Binding SearchBarBorderColor", xaml, StringComparison.Ordinal);
            Assert.Contains("SelectedColor=\"{Binding SearchBarInnerBorderColor", xaml, StringComparison.Ordinal);
            Assert.Contains("SelectedColor=\"{Binding HoverColor", xaml, StringComparison.Ordinal);
            Assert.Contains("SelectedColor=\"{Binding SelectedColor", xaml, StringComparison.Ordinal);
            Assert.Contains("Selected text", xaml, StringComparison.Ordinal);
            Assert.Contains("UsingAlphaChannel", xaml, StringComparison.Ordinal);
        }

        private static ThemeDesignerViewModel CreateViewModel(
            FakeSettingsService? settingsService = null,
            FakeThemeService? themeService = null,
            FakeFileDialogService? fileDialogs = null)
        {
            return new ThemeDesignerViewModel(
                settingsService ?? new FakeSettingsService(),
                themeService ?? new FakeThemeService(),
                fileDialogs ?? new FakeFileDialogService(),
                new FakeDialogService());
        }

        private sealed class FakeThemeService : IThemeService
        {
            public AppThemeSettings? LastAppliedCustomTheme { get; private set; }
            public void ApplyTheme(string? theme) { }
            public void ApplyCustomTheme(AppThemeSettings? settings) => LastAppliedCustomTheme = settings;
        }

        private sealed class FakeFileDialogService : IFileDialogService
        {
            public string? OpenPath { get; set; }
            public string? SavePath { get; set; }
            public string? LastOpenFilter { get; private set; }
            public string? LastSaveFilter { get; private set; }

            public string? OpenFile(string filter, string? initialDirectory = null)
            {
                LastOpenFilter = filter;
                return OpenPath;
            }

            public string? SaveFile(string filter, string? initialDirectory = null)
            {
                LastSaveFilter = filter;
                return SavePath;
            }

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

        private sealed class FakeSettingsService : ISettingsService
        {
            private readonly Dictionary<string, string> _settings = new();
            public int SaveThemeCalls { get; private set; }

            public event EventHandler<IDictionary<ItemDetailField, bool>>? ItemDetailVisibilityChanged;
            public event EventHandler<double>? ItemCardSizeChanged;

            public Task SaveSettingAsync(string key, string value, CancellationToken cancellationToken = default)
            {
                _settings[key] = value;
                return Task.CompletedTask;
            }

            public Task<string?> GetSettingAsync(string? key, CancellationToken cancellationToken = default)
                => Task.FromResult(key != null && _settings.TryGetValue(key, out var value) ? value : null);

            public Task<Dictionary<string, string>> GetAllSettingsAsync(CancellationToken cancellationToken = default)
                => Task.FromResult(new Dictionary<string, string>(_settings));

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

            public Task<string?> GetThemeAsync(CancellationToken cancellationToken = default) => Task.FromResult<string?>("Light");

            public Task SaveThemeAsync(string theme, CancellationToken cancellationToken = default)
            {
                SaveThemeCalls++;
                return Task.CompletedTask;
            }

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

        private static string ReadRepositoryFile(params string[] relativePathParts)
        {
            var directory = new DirectoryInfo(AppContext.BaseDirectory);
            while (directory != null && !File.Exists(Path.Combine(directory.FullName, "InventoryManagementApp.sln")))
                directory = directory.Parent;

            Assert.NotNull(directory);
            var path = Path.Combine(directory!.FullName, Path.Combine(relativePathParts));
            Assert.True(File.Exists(path), $"Expected repository file at {path}");
            return File.ReadAllText(path);
        }

        private static string CreateTempImage(string extension)
        {
            var path = Path.Combine(Path.GetTempPath(), $"inventory-theme-background-{Guid.NewGuid():N}{extension}");
            File.WriteAllBytes(path, new byte[] { 1, 2, 3, 4, 5 });
            return path;
        }
    }
}
