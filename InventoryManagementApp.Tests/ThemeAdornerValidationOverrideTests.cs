using System;
using System.IO;
using Xunit;

namespace InventoryManagementApp.Tests
{
    public class ThemeAdornerValidationOverrideTests
    {
        [Fact]
        public void AppResourcesLoadAdornerValidationOverridesAfterTextHierarchyOverrides()
        {
            var appXaml = ReadRepoFile("InventoryManagementApp", "App.xaml");

            var textHierarchyIndex = appXaml.IndexOf("Theme.TextHierarchyOverrides.xaml", StringComparison.Ordinal);
            var adornerIndex = appXaml.IndexOf("Theme.AdornerValidationOverrides.xaml", StringComparison.Ordinal);
            var convertersIndex = appXaml.IndexOf("Resources/Converters.xaml", StringComparison.Ordinal);

            Assert.True(textHierarchyIndex >= 0, "Text hierarchy overrides must stay in the app resource load order.");
            Assert.True(adornerIndex > textHierarchyIndex, "Adorner and validation overrides should load after text hierarchy overrides.");
            Assert.True(convertersIndex > adornerIndex, "Adorner and validation overrides should remain with the final theme layers before converters/templates.");
        }

        [Fact]
        public void AdornerValidationOverridesThemeValidationFramesThroughAdminTokens()
        {
            var xaml = ReadRepoFile("InventoryManagementApp", "Resources", "Theme.AdornerValidationOverrides.xaml");

            Assert.Contains("ThemeValidationErrorTemplate", xaml, StringComparison.Ordinal);
            Assert.Contains("AdornedElementPlaceholder", xaml, StringComparison.Ordinal);
            Assert.Contains("Validation.ErrorTemplate", xaml, StringComparison.Ordinal);
            Assert.Contains("ThemeControlBorderThickness", xaml, StringComparison.Ordinal);
            Assert.Contains("ThemeInputCornerRadius", xaml, StringComparison.Ordinal);
            Assert.Contains("ThemeControlShadow", xaml, StringComparison.Ordinal);
            Assert.Contains("ErrorBrush", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void AdornerValidationOverridesCoverOuterPresenterChrome()
        {
            var xaml = ReadRepoFile("InventoryManagementApp", "Resources", "Theme.AdornerValidationOverrides.xaml");

            Assert.Contains("TargetType=\"AdornerDecorator\"", xaml, StringComparison.Ordinal);
            Assert.Contains("TargetType=\"AdornerLayer\"", xaml, StringComparison.Ordinal);
            Assert.Contains("TargetType=\"BulletDecorator\"", xaml, StringComparison.Ordinal);
            Assert.Contains("TargetType=\"Viewbox\"", xaml, StringComparison.Ordinal);
            Assert.Contains("TargetType=\"primitives:ToolBarOverflowPanel\"", xaml, StringComparison.Ordinal);
            Assert.Contains("TargetType=\"primitives:StatusBarPanel\"", xaml, StringComparison.Ordinal);
            Assert.Contains("TargetType=\"primitives:DataGridDetailsPresenter\"", xaml, StringComparison.Ordinal);
            Assert.Contains("TargetType=\"primitives:DataGridCellsPresenter\"", xaml, StringComparison.Ordinal);
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
