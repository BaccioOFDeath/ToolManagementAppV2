using System;
using System.IO;
using Xunit;

namespace InventoryManagementApp.Tests.ViewModels
{
    public class SettingsThemeCustomizationXamlTests
    {
        [Fact]
        public void SettingsPage_ExposesAdminThemeSelection()
        {
            var xaml = ReadRepositoryFile("InventoryManagementApp", "Views", "Pages", "SettingsPage.xaml");

            Assert.Contains("Admin Settings Workbench", xaml, StringComparison.Ordinal);
            Assert.Contains("Text=\"Theme\"", xaml, StringComparison.Ordinal);
            Assert.Contains("SelectedItem=\"{Binding Theme}\"", xaml, StringComparison.Ordinal);
            Assert.Contains("ItemsSource=\"{Binding ThemeOptions}\"", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void App_LoadsThemeCustomizationDictionaryBeforeSharedStyles()
        {
            var xaml = ReadRepositoryFile("InventoryManagementApp", "App.xaml");

            var themeCustomizationIndex = xaml.IndexOf("Resources/Theme.Customization.xaml", StringComparison.Ordinal);
            var stylesIndex = xaml.IndexOf("Resources/Styles.xaml", StringComparison.Ordinal);
            var desktopShellIndex = xaml.IndexOf("Resources/DesktopShell.xaml", StringComparison.Ordinal);

            Assert.True(themeCustomizationIndex > 0, "Theme customization resources should be merged into App.xaml.");
            Assert.True(themeCustomizationIndex < stylesIndex, "Theme customization resources must load before shared styles.");
            Assert.True(themeCustomizationIndex < desktopShellIndex, "Theme customization resources must load before shell styles.");
        }

        [Fact]
        public void ThemeCustomizationResources_DefineFullAppVisualKnobs()
        {
            var xaml = ReadRepositoryFile("InventoryManagementApp", "Resources", "Theme.Customization.xaml");

            Assert.Contains("ThemeBorderThickness", xaml, StringComparison.Ordinal);
            Assert.Contains("ThemeBorderlessThickness", xaml, StringComparison.Ordinal);
            Assert.Contains("ThemeCardCornerRadius", xaml, StringComparison.Ordinal);
            Assert.Contains("ThemeButtonCornerRadius", xaml, StringComparison.Ordinal);
            Assert.Contains("ThemeTransparentBrush", xaml, StringComparison.Ordinal);
            Assert.Contains("ThemeGlassSurfaceBrush", xaml, StringComparison.Ordinal);
            Assert.Contains("ThemeAppBackgroundOverlayBrush", xaml, StringComparison.Ordinal);
            Assert.Contains("ThemeNoShadow", xaml, StringComparison.Ordinal);
            Assert.Contains("ThemeSurfaceShadow", xaml, StringComparison.Ordinal);
            Assert.Contains("ThemeDeepShadow", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void SharedStyles_UseThemeShapeTransparencyAndDisableDefaultShadows()
        {
            var polishedXaml = ReadRepositoryFile("InventoryManagementApp", "Resources", "PolishedVisualHierarchy.xaml");
            var shellXaml = ReadRepositoryFile("InventoryManagementApp", "Resources", "DesktopShell.xaml");

            Assert.Contains("ThemeCardCornerRadius", polishedXaml, StringComparison.Ordinal);
            Assert.Contains("ThemePanelCornerRadius", polishedXaml, StringComparison.Ordinal);
            Assert.Contains("Effect\" Value=\"{x:Null}\"", polishedXaml, StringComparison.Ordinal);
            Assert.Contains("GlassSurfaceBrush", polishedXaml, StringComparison.Ordinal);
            Assert.Contains("GlassSurfaceAltBrush", polishedXaml, StringComparison.Ordinal);

            Assert.Contains("ThemeButtonCornerRadius", shellXaml, StringComparison.Ordinal);
            Assert.Contains("ThemeControlBorderThickness", shellXaml, StringComparison.Ordinal);
            Assert.Contains("ThemeBorderlessThickness", shellXaml, StringComparison.Ordinal);
            Assert.Contains("TransparentSurfaceBrush", shellXaml, StringComparison.Ordinal);
        }

        [Fact]
        public void LightAndDarkPalettes_ExposeTransparentAndGlassBrushes()
        {
            var lightXaml = ReadRepositoryFile("InventoryManagementApp", "Resources", "Colors.Light.xaml");
            var darkXaml = ReadRepositoryFile("InventoryManagementApp", "Resources", "Colors.Dark.xaml");

            foreach (var xaml in new[] { lightXaml, darkXaml })
            {
                Assert.Contains("TransparentSurfaceBrush", xaml, StringComparison.Ordinal);
                Assert.Contains("GlassSurfaceBrush", xaml, StringComparison.Ordinal);
                Assert.Contains("GlassSurfaceAltBrush", xaml, StringComparison.Ordinal);
                Assert.Contains("AppBackgroundTintBrush", xaml, StringComparison.Ordinal);
            }
        }

        static string ReadRepositoryFile(params string[] relativePathParts)
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
