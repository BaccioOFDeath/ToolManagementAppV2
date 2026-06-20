using System;
using System.IO;
using Xunit;

namespace InventoryManagementApp.Tests.Resources
{
    public class ThemeOverlayDocumentOverrideTests
    {
        [Fact]
        public void App_LoadsOverlayDocumentOverridesAfterSpecialSurfaces()
        {
            var xaml = ReadRepositoryFile("InventoryManagementApp", "App.xaml");

            var specialSurfaceIndex = xaml.IndexOf("Resources/Theme.SpecialSurfaceOverrides.xaml", StringComparison.Ordinal);
            var overlayDocumentIndex = xaml.IndexOf("Resources/Theme.OverlayDocumentOverrides.xaml", StringComparison.Ordinal);
            var convertersIndex = xaml.IndexOf("Resources/Converters.xaml", StringComparison.Ordinal);

            Assert.True(specialSurfaceIndex >= 0, "Special surface overrides should remain loaded.");
            Assert.True(overlayDocumentIndex > specialSurfaceIndex, "Overlay/document overrides should be the final theme coverage layer.");
            Assert.True(convertersIndex > overlayDocumentIndex, "Converters should remain after visual resources.");
        }

        [Fact]
        public void OverlayDocumentOverrides_ExtendAdminThemesToPopupMenuAndDisclosureChrome()
        {
            var xaml = ReadRepositoryFile("InventoryManagementApp", "Resources", "Theme.OverlayDocumentOverrides.xaml");

            Assert.Contains("TargetType=\"Menu\"", xaml, StringComparison.Ordinal);
            Assert.Contains("TargetType=\"ContextMenu\"", xaml, StringComparison.Ordinal);
            Assert.Contains("TargetType=\"MenuItem\"", xaml, StringComparison.Ordinal);
            Assert.Contains("TargetType=\"ToolTip\"", xaml, StringComparison.Ordinal);
            Assert.Contains("TargetType=\"GroupBox\"", xaml, StringComparison.Ordinal);
            Assert.Contains("TargetType=\"Expander\"", xaml, StringComparison.Ordinal);
            Assert.Contains("TargetType=\"Separator\"", xaml, StringComparison.Ordinal);
            Assert.Contains("Property=\"IsHighlighted\" Value=\"True\"", xaml, StringComparison.Ordinal);
            Assert.Contains("Property=\"IsChecked\" Value=\"True\"", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void OverlayDocumentOverrides_ExtendAdminThemesToDocumentTextStructureAndAdorners()
        {
            var xaml = ReadRepositoryFile("InventoryManagementApp", "Resources", "Theme.OverlayDocumentOverrides.xaml");

            Assert.Contains("TargetType=\"FlowDocument\"", xaml, StringComparison.Ordinal);
            Assert.Contains("TargetType=\"Paragraph\"", xaml, StringComparison.Ordinal);
            Assert.Contains("TargetType=\"Run\"", xaml, StringComparison.Ordinal);
            Assert.Contains("TargetType=\"Span\"", xaml, StringComparison.Ordinal);
            Assert.Contains("TargetType=\"Section\"", xaml, StringComparison.Ordinal);
            Assert.Contains("TargetType=\"List\"", xaml, StringComparison.Ordinal);
            Assert.Contains("TargetType=\"ListItem\"", xaml, StringComparison.Ordinal);
            Assert.Contains("TargetType=\"Hyperlink\"", xaml, StringComparison.Ordinal);
            Assert.Contains("TargetType=\"Table\"", xaml, StringComparison.Ordinal);
            Assert.Contains("TargetType=\"TableCell\"", xaml, StringComparison.Ordinal);
            Assert.Contains("TargetType=\"documents:AdornerDecorator\"", xaml, StringComparison.Ordinal);
            Assert.Contains("TargetType=\"documents:AdornerLayer\"", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void OverlayDocumentOverrides_UseAdminThemeTokensForTransparencyTypographyBordersAndDepth()
        {
            var xaml = ReadRepositoryFile("InventoryManagementApp", "Resources", "Theme.OverlayDocumentOverrides.xaml");

            Assert.Contains("{DynamicResource TransparentSurfaceBrush}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource BackgroundBrush}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource ThemePopupSurfaceBrush}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource ThemeDialogSurfaceBrush}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource GlassSurfaceBrush}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource ForegroundBrush}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource AccentBrush}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource BorderBrushAlt}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource ThemeBorderThickness}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource ThemeControlBorderThickness}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource ThemeSubtleBorderThickness}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource ThemeBorderlessThickness}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource ThemeGridLineBrush}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource ThemeFontFamily}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource ThemeBodyFontSize}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource ThemeControlMinHeight}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource CardPadding}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource ControlPadding}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource ThemeSurfaceShadow}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource ThemeRaisedShadow}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource ItemHoverBrush}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource ItemSelectedBrush}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource ThemeDisabledOpacity}", xaml, StringComparison.Ordinal);
            Assert.Contains("FocusVisualStyle\" Value=\"{StaticResource DefaultFocusVisual}\"", xaml, StringComparison.Ordinal);
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
