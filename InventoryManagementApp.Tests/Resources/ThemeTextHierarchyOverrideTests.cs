using System;
using System.IO;
using Xunit;

namespace InventoryManagementApp.Tests.Resources
{
    public class ThemeTextHierarchyOverrideTests
    {
        [Fact]
        public void App_LoadsTextHierarchyOverridesAfterPopupChromeLayer()
        {
            var xaml = ReadRepositoryFile("InventoryManagementApp", "App.xaml");

            var popupIndex = xaml.IndexOf("Resources/Theme.PopupChromeOverrides.xaml", StringComparison.Ordinal);
            var textIndex = xaml.IndexOf("Resources/Theme.TextHierarchyOverrides.xaml", StringComparison.Ordinal);
            var convertersIndex = xaml.IndexOf("Resources/Converters.xaml", StringComparison.Ordinal);

            Assert.True(popupIndex >= 0, "Popup chrome overrides should remain loaded.");
            Assert.True(textIndex > popupIndex, "Text hierarchy overrides should load after popup/status chrome.");
            Assert.True(convertersIndex > textIndex, "Converters should remain after the final visual override layer.");
        }

        [Fact]
        public void TextHierarchyOverrides_ReclaimSharedTextBlockRolesForAdminThemes()
        {
            var xaml = ReadRepositoryFile("InventoryManagementApp", "Resources", "Theme.TextHierarchyOverrides.xaml");

            Assert.Contains("TargetType=\"TextBlock\"", xaml, StringComparison.Ordinal);
            Assert.Contains("x:Key=\"ErrorTextBlock\"", xaml, StringComparison.Ordinal);
            Assert.Contains("x:Key=\"SectionHeader\"", xaml, StringComparison.Ordinal);
            Assert.Contains("x:Key=\"HeadingTextBlock\"", xaml, StringComparison.Ordinal);
            Assert.Contains("x:Key=\"SubheadingTextBlock\"", xaml, StringComparison.Ordinal);
            Assert.Contains("x:Key=\"DialogMessageTextBlock\"", xaml, StringComparison.Ordinal);
            Assert.Contains("x:Key=\"PageTitleTextBlock\"", xaml, StringComparison.Ordinal);
            Assert.Contains("x:Key=\"CaptionTextBlock\"", xaml, StringComparison.Ordinal);
            Assert.Contains("x:Key=\"LabelTextBlock\"", xaml, StringComparison.Ordinal);
            Assert.Contains("x:Key=\"ListItemTitleTextBlock\"", xaml, StringComparison.Ordinal);
            Assert.Contains("x:Key=\"StatisticValueTextBlock\"", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void TextHierarchyOverrides_UseAdminThemeTokensForFontScaleColorAndRendering()
        {
            var xaml = ReadRepositoryFile("InventoryManagementApp", "Resources", "Theme.TextHierarchyOverrides.xaml");

            Assert.Contains("{DynamicResource ThemeFontFamily}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource ThemeBodyFontSize}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource ThemeSectionFontSize}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource ThemeTitleFontSize}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource ThemeCaptionFontSize}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource ForegroundBrush}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource ForegroundMutedBrush}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource AccentBrush}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource ErrorBrush}", xaml, StringComparison.Ordinal);
            Assert.Contains("TextOptions.TextFormattingMode", xaml, StringComparison.Ordinal);
            Assert.Contains("TextOptions.TextRenderingMode", xaml, StringComparison.Ordinal);
            Assert.Contains("TextOptions.TextHintingMode", xaml, StringComparison.Ordinal);
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
    }
}
