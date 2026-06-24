using System;
using System.IO;
using Xunit;

namespace InventoryManagementApp.Tests.Resources
{
    public class ThemeRangeScrollChromeOverrideTests
    {
        [Fact]
        public void App_LoadsRangeScrollChromeOverridesAfterFinalInputLayer()
        {
            var xaml = ReadRepositoryFile("InventoryManagementApp", "App.xaml");

            var finalInputIndex = xaml.IndexOf("Resources/Theme.FinalContentInputOverrides.xaml", StringComparison.Ordinal);
            var rangeScrollIndex = xaml.IndexOf("Resources/Theme.RangeScrollChromeOverrides.xaml", StringComparison.Ordinal);
            var convertersIndex = xaml.IndexOf("Resources/Converters.xaml", StringComparison.Ordinal);

            Assert.True(finalInputIndex >= 0, "Final content/input overrides should remain loaded.");
            Assert.True(rangeScrollIndex > finalInputIndex, "Range and scroll chrome overrides should load after final input coverage.");
            Assert.True(convertersIndex > rangeScrollIndex, "Converters should remain after the final visual override layer.");
        }

        [Fact]
        public void RangeScrollChromeOverrides_ExtendAdminThemesToRangeScrollAndDragPrimitives()
        {
            var xaml = ReadRepositoryFile("InventoryManagementApp", "Resources", "Theme.RangeScrollChromeOverrides.xaml");

            Assert.Contains("TargetType=\"ScrollViewer\"", xaml, StringComparison.Ordinal);
            Assert.Contains("TargetType=\"ScrollBar\"", xaml, StringComparison.Ordinal);
            Assert.Contains("TargetType=\"Slider\"", xaml, StringComparison.Ordinal);
            Assert.Contains("TargetType=\"ProgressBar\"", xaml, StringComparison.Ordinal);
            Assert.Contains("TargetType=\"GridSplitter\"", xaml, StringComparison.Ordinal);
            Assert.Contains("TargetType=\"primitives:Thumb\"", xaml, StringComparison.Ordinal);
            Assert.Contains("TargetType=\"primitives:RepeatButton\"", xaml, StringComparison.Ordinal);
            Assert.Contains("ThemeRangeTrackBorder", xaml, StringComparison.Ordinal);
            Assert.Contains("ThemeRangeFillBorder", xaml, StringComparison.Ordinal);
            Assert.Contains("ThemeRangeThumbStyle", xaml, StringComparison.Ordinal);
            Assert.Contains("ThemeScrollLineButtonStyle", xaml, StringComparison.Ordinal);
            Assert.Contains("ThemeScrollPageButtonStyle", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void RangeScrollChromeOverrides_UseAdminThemeTokensForTransparencyDepthShapeAndInteraction()
        {
            var xaml = ReadRepositoryFile("InventoryManagementApp", "Resources", "Theme.RangeScrollChromeOverrides.xaml");

            Assert.Contains("{DynamicResource TransparentSurfaceBrush}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource ProgressBarBackgroundBrush}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource AccentBrush}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource BorderBrushAlt}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource BtnBg}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource BtnBgHover}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource BtnBorder}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource BtnFg}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource ThemeBorderlessThickness}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource ThemeControlBorderThickness}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource ThemeButtonCornerRadius}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource ThemeInputCornerRadius}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource ThemeControlMinHeight}", xaml, StringComparison.Ordinal);
            Assert.Contains("{x:Null}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource ItemHoverBrush}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource ItemSelectedBrush}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource NavButtonPressedBrush}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource ThemeDisabledOpacity}", xaml, StringComparison.Ordinal);
            Assert.Contains("FocusVisualStyle\" Value=\"{StaticResource DefaultFocusVisual}\"", xaml, StringComparison.Ordinal);
            Assert.Contains("RecognizesAccessKey=\"True\"", xaml, StringComparison.Ordinal);
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
