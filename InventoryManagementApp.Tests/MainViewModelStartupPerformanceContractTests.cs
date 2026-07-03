using System;
using System.IO;
using Xunit;

namespace InventoryManagementApp.Tests
{
    public class MainViewModelStartupPerformanceContractTests
    {
        [Fact]
        public void MainViewModel_LoadsShellBrandingWithoutBlockingConstructor()
        {
            var source = ReadRepoFile("InventoryManagementApp", "ViewModels", "MainViewModel.cs");

            Assert.Contains("Task? _shellBrandingLoadTask;", source, StringComparison.Ordinal);
            Assert.Contains("_shellBrandingLoadTask = LoadShellBrandingAsync();", source, StringComparison.Ordinal);
            Assert.Contains("async Task LoadShellBrandingAsync()", source, StringComparison.Ordinal);
            Assert.DoesNotContain("GetSettingAsync(\"CompanyLogoPath\").GetAwaiter().GetResult()", source, StringComparison.Ordinal);
            Assert.DoesNotContain("GetSettingAsync(\"ApplicationName\").GetAwaiter().GetResult()", source, StringComparison.Ordinal);
            Assert.DoesNotContain(".Result;", source, StringComparison.Ordinal);
        }

        [Fact]
        public void MainViewModel_LoadsBrandingSettingsTogetherAndKeepsDefaultTitleVisible()
        {
            var source = ReadRepoFile("InventoryManagementApp", "ViewModels", "MainViewModel.cs");

            Assert.Contains("var logoPathTask = _settingsService.GetSettingAsync(\"CompanyLogoPath\");", source, StringComparison.Ordinal);
            Assert.Contains("var appNameTask = _settingsService.GetSettingAsync(\"ApplicationName\");", source, StringComparison.Ordinal);
            Assert.Contains("await Task.WhenAll(logoPathTask, appNameTask).ConfigureAwait(true);", source, StringComparison.Ordinal);
            Assert.Contains("public string WindowTitle => string.IsNullOrWhiteSpace(ApplicationName)", source, StringComparison.Ordinal);
            Assert.Contains("? $\"{LabelProvider.Instance.ItemLabelPlural} Management\"", source, StringComparison.Ordinal);
            Assert.Contains(": ApplicationName;", source, StringComparison.Ordinal);
        }

        [Fact]
        public void MainViewModel_AppliesOnlyNonBlankBrandingAndLogsReadFailures()
        {
            var source = ReadRepoFile("InventoryManagementApp", "ViewModels", "MainViewModel.cs");

            Assert.Contains("if (!string.IsNullOrWhiteSpace(logoPath))", source, StringComparison.Ordinal);
            Assert.Contains("CompanyLogoPath = logoPath;", source, StringComparison.Ordinal);
            Assert.Contains("if (!string.IsNullOrWhiteSpace(appName))", source, StringComparison.Ordinal);
            Assert.Contains("ApplicationName = appName;", source, StringComparison.Ordinal);
            Assert.Contains("catch (Exception ex)", source, StringComparison.Ordinal);
            Assert.Contains("_logger.LogWarning(ex, \"Failed to load shell branding settings\");", source, StringComparison.Ordinal);
        }

        [Fact]
        public void MainViewModel_PreservesLiveSettingsBrandingUpdates()
        {
            var source = ReadRepoFile("InventoryManagementApp", "ViewModels", "MainViewModel.cs");

            Assert.Contains("else if (e.PropertyName == nameof(SettingsViewModel.CompanyLogoPath))", source, StringComparison.Ordinal);
            Assert.Contains("CompanyLogoPath = Settings.CompanyLogoPath;", source, StringComparison.Ordinal);
            Assert.Contains("else if (e.PropertyName == nameof(SettingsViewModel.ApplicationName))", source, StringComparison.Ordinal);
            Assert.Contains("ApplicationName = Settings.ApplicationName;", source, StringComparison.Ordinal);
        }

        private static string ReadRepoFile(params string[] parts)
        {
            var directory = AppContext.BaseDirectory;

            while (!string.IsNullOrEmpty(directory))
            {
                var candidate = Path.Combine(directory, Path.Combine(parts));
                if (File.Exists(candidate))
                    return File.ReadAllText(candidate);

                var parent = Directory.GetParent(directory);
                if (parent is null)
                    break;

                directory = parent.FullName;
            }

            throw new FileNotFoundException($"Could not find repository file: {Path.Combine(parts)}");
        }
    }
}
