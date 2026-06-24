using System;
using System.IO;
using Xunit;

namespace InventoryManagementApp.Tests.Resources
{
    public class ThemeFinalContentInputOverrideTests
    {
        [Fact]
        public void App_LoadsFinalContentInputOverridesAfterDocumentLayer()
        {
            var xaml = ReadRepositoryFile("InventoryManagementApp", "App.xaml");

            var documentIndex = xaml.IndexOf("Resources/Theme.OverlayDocumentOverrides.xaml", StringComparison.Ordinal);
            var finalInputIndex = xaml.IndexOf("Resources/Theme.FinalContentInputOverrides.xaml", StringComparison.Ordinal);
            var convertersIndex = xaml.IndexOf("Resources/Converters.xaml", StringComparison.Ordinal);

            Assert.True(documentIndex >= 0, "Document and overlay theme coverage should still be loaded.");
            Assert.True(finalInputIndex > documentIndex, "Final content/input overrides should load after document and overlay coverage.");
            Assert.True(convertersIndex > finalInputIndex, "Converters should remain after the final visual override layer.");
        }

        [Fact]
        public void FinalContentInputOverrides_ThemeRemainingTextSecureInputAndButtonPrimitiveSurfaces()
        {
            var xaml = ReadRepositoryFile("InventoryManagementApp", "Resources", "Theme.FinalContentInputOverrides.xaml");

            Assert.Contains("TargetType=\"Label\"", xaml, StringComparison.Ordinal);
            Assert.Contains("TargetType=\"AccessText\"", xaml, StringComparison.Ordinal);
            Assert.Contains("TargetType=\"PasswordBox\"", xaml, StringComparison.Ordinal);
            Assert.Contains("TargetType=\"RichTextBox\"", xaml, StringComparison.Ordinal);
            Assert.Contains("TargetType=\"primitives:ToggleButton\"", xaml, StringComparison.Ordinal);
            Assert.Contains("TargetType=\"primitives:RepeatButton\"", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void FinalContentInputOverrides_UseAdminThemeTokensForTransparencyShapeDepthAndInteraction()
        {
            var xaml = ReadRepositoryFile("InventoryManagementApp", "Resources", "Theme.FinalContentInputOverrides.xaml");

            Assert.Contains("{DynamicResource TransparentSurfaceBrush}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource TextBoxBackgroundBrush}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource BtnBg}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource BtnBgHover}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource BtnBorder}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource ForegroundBrush}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource BtnFg}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource AccentBrush}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource BorderBrushAlt}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource ThemeControlBorderThickness}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource ThemeFontFamily}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource ThemeBodyFontSize}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource ThemeControlMinHeight}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource ControlPadding}", xaml, StringComparison.Ordinal);
            Assert.Contains("{x:Null}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource ThemeDisabledOpacity}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource ItemSelectedBrush}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource ItemSelectedForegroundBrush}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource NavButtonPressedBrush}", xaml, StringComparison.Ordinal);
            Assert.Contains("{StaticResource DefaultFocusVisual}", xaml, StringComparison.Ordinal);
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
