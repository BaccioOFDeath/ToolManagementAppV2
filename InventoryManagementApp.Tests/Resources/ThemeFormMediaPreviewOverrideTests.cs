using System;
using System.IO;
using Xunit;

namespace InventoryManagementApp.Tests.Resources
{
    public class ThemeFormMediaPreviewOverrideTests
    {
        [Fact]
        public void App_LoadsFormMediaPreviewOverridesAfterFullCustomizationOverrides()
        {
            var xaml = ReadRepositoryFile("InventoryManagementApp", "App.xaml");

            var fullCustomizationIndex = xaml.IndexOf("Resources/Theme.FullCustomizationOverrides.xaml", StringComparison.Ordinal);
            var formMediaIndex = xaml.IndexOf("Resources/Theme.FormMediaPreviewOverrides.xaml", StringComparison.Ordinal);
            var convertersIndex = xaml.IndexOf("Resources/Converters.xaml", StringComparison.Ordinal);

            Assert.True(fullCustomizationIndex >= 0, "Full customization overrides should remain loaded.");
            Assert.True(formMediaIndex > fullCustomizationIndex, "Form/media preview overrides must load after full customization overrides.");
            Assert.True(convertersIndex > formMediaIndex, "Converters should remain after visual resources.");
        }

        [Fact]
        public void FormMediaPreviewOverrides_ThemeRemainingFormMediaAndPreviewChrome()
        {
            var xaml = ReadRepositoryFile("InventoryManagementApp", "Resources", "Theme.FormMediaPreviewOverrides.xaml");

            Assert.Contains("x:Key=\"ThemeMediaPreviewFrame\"", xaml, StringComparison.Ordinal);
            Assert.Contains("TargetType=\"PasswordBox\"", xaml, StringComparison.Ordinal);
            Assert.Contains("TargetType=\"CheckBox\"", xaml, StringComparison.Ordinal);
            Assert.Contains("TargetType=\"Slider\"", xaml, StringComparison.Ordinal);
            Assert.Contains("TargetType=\"ProgressBar\"", xaml, StringComparison.Ordinal);
            Assert.Contains("TargetType=\"Label\"", xaml, StringComparison.Ordinal);
            Assert.Contains("TargetType=\"Image\"", xaml, StringComparison.Ordinal);
            Assert.Contains("TargetType=\"DocumentViewer\"", xaml, StringComparison.Ordinal);
            Assert.Contains("TargetType=\"FlowDocumentScrollViewer\"", xaml, StringComparison.Ordinal);
            Assert.Contains("TargetType=\"FlowDocument\"", xaml, StringComparison.Ordinal);
            Assert.Contains("TargetType=\"Section\"", xaml, StringComparison.Ordinal);
            Assert.Contains("TargetType=\"Paragraph\"", xaml, StringComparison.Ordinal);
            Assert.Contains("TargetType=\"Hyperlink\"", xaml, StringComparison.Ordinal);
            Assert.Contains("TargetType=\"RichTextBox\"", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void FormMediaPreviewOverrides_UseAdminThemeTokensForTransparencyBordersAndDepth()
        {
            var xaml = ReadRepositoryFile("InventoryManagementApp", "Resources", "Theme.FormMediaPreviewOverrides.xaml");

            Assert.Contains("{DynamicResource TransparentSurfaceBrush}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource TextBoxBackgroundBrush}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource ThemeDialogSurfaceBrush}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource ProgressBarBackgroundBrush}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource AccentBrush}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource BorderBrushAlt}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource ThemeBorderThickness}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource ThemeControlBorderThickness}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource ThemeBorderlessThickness}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource ThemeCardCornerRadius}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource ThemeControlShadow}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource ThemeRaisedShadow}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource ThemeDisabledOpacity}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource ThemeControlMinHeight}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource ThemeFontFamily}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource ItemHoverBrush}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource ItemSelectedBrush}", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void FormMediaPreviewOverrides_PreserveFinalInteractionAndReadabilityChrome()
        {
            var xaml = ReadRepositoryFile("InventoryManagementApp", "Resources", "Theme.FormMediaPreviewOverrides.xaml");

            Assert.Contains("DefaultFocusVisual", xaml, StringComparison.Ordinal);
            Assert.Contains("AutoToolTipPlacement", xaml, StringComparison.Ordinal);
            Assert.Contains("AutoToolTipPrecision", xaml, StringComparison.Ordinal);
            Assert.Contains("IsMoveToPointEnabled", xaml, StringComparison.Ordinal);
            Assert.Contains("TextBlock.TextTrimming", xaml, StringComparison.Ordinal);
            Assert.Contains("TextBlock.TextWrapping", xaml, StringComparison.Ordinal);
            Assert.Contains("SnapsToDevicePixels", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void FormMediaPreviewOverrides_ThemeDocumentTextAndLinksForTransparentBackgrounds()
        {
            var xaml = ReadRepositoryFile("InventoryManagementApp", "Resources", "Theme.FormMediaPreviewOverrides.xaml");

            Assert.Contains("TargetType=\"FlowDocument\"", xaml, StringComparison.Ordinal);
            Assert.Contains("TargetType=\"Section\"", xaml, StringComparison.Ordinal);
            Assert.Contains("TargetType=\"Paragraph\"", xaml, StringComparison.Ordinal);
            Assert.Contains("TargetType=\"Hyperlink\"", xaml, StringComparison.Ordinal);
            Assert.Contains("Foreground\" Value=\"{DynamicResource ForegroundBrush}\"", xaml, StringComparison.Ordinal);
            Assert.Contains("Foreground\" Value=\"{DynamicResource AccentBrush}\"", xaml, StringComparison.Ordinal);
            Assert.Contains("TextDecorations\" Value=\"{x:Null}\"", xaml, StringComparison.Ordinal);
            Assert.Contains("TextDecorations\" Value=\"Underline\"", xaml, StringComparison.Ordinal);
            Assert.Contains("PagePadding\" Value=\"{DynamicResource CardPadding}\"", xaml, StringComparison.Ordinal);
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
