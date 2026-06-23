using System;
using System.IO;
using Xunit;

namespace InventoryManagementApp.Tests.Resources
{
    public class ThemeAdminDesignerCoverageOverrideTests
    {
        [Fact]
        public void App_LoadsAdminDesignerCoverageOverridesAsLastThemeLayer()
        {
            var xaml = ReadRepositoryFile("InventoryManagementApp", "App.xaml");

            var fullCustomizationIndex = xaml.IndexOf("Resources/Theme.FullCustomizationOverrides.xaml", StringComparison.Ordinal);
            var controlCustomizationIndex = xaml.IndexOf("Resources/Theme.ControlCustomizationOverrides.xaml", StringComparison.Ordinal);
            var formMediaIndex = xaml.IndexOf("Resources/Theme.FormMediaPreviewOverrides.xaml", StringComparison.Ordinal);
            var adminCoverageIndex = xaml.IndexOf("Resources/Theme.AdminDesignerCoverageOverrides.xaml", StringComparison.Ordinal);
            var navigationChromeIndex = xaml.IndexOf("Resources/Theme.NavigationChromeOverrides.xaml", StringComparison.Ordinal);
            var layoutPresenterIndex = xaml.IndexOf("Resources/Theme.LayoutPresenterOverrides.xaml", StringComparison.Ordinal);
            var specialSurfaceIndex = xaml.IndexOf("Resources/Theme.SpecialSurfaceOverrides.xaml", StringComparison.Ordinal);
            var convertersIndex = xaml.IndexOf("Resources/Converters.xaml", StringComparison.Ordinal);

            Assert.True(fullCustomizationIndex >= 0, "Full customization overrides should remain loaded.");
            Assert.True(controlCustomizationIndex > fullCustomizationIndex, "Control overrides should load after full customization resources.");
            Assert.True(formMediaIndex > controlCustomizationIndex, "Form/media overrides should load after control overrides.");
            Assert.True(adminCoverageIndex > formMediaIndex, "Admin coverage overrides should remain after form/media resources.");
            Assert.True(navigationChromeIndex > adminCoverageIndex, "Navigation chrome overrides should load after the broad admin coverage layer.");
            Assert.True(layoutPresenterIndex > navigationChromeIndex, "Layout presenter overrides should load after navigation chrome resources.");
            Assert.True(specialSurfaceIndex > layoutPresenterIndex, "Special surface overrides should load after layout presenter resources.");
            Assert.True(convertersIndex > specialSurfaceIndex, "Converters should remain after visual resources.");
        }

        [Fact]
        public void AdminDesignerCoverageOverrides_ExtendWholeAppChromeToRemainingContainers()
        {
            var xaml = ReadRepositoryFile("InventoryManagementApp", "Resources", "Theme.AdminDesignerCoverageOverrides.xaml");

            Assert.Contains("TargetType=\"Border\"", xaml, StringComparison.Ordinal);
            Assert.Contains("TargetType=\"ScrollViewer\"", xaml, StringComparison.Ordinal);
            Assert.Contains("TargetType=\"ScrollBar\"", xaml, StringComparison.Ordinal);
            Assert.Contains("TargetType=\"ListBox\"", xaml, StringComparison.Ordinal);
            Assert.Contains("TargetType=\"ListView\"", xaml, StringComparison.Ordinal);
            Assert.Contains("TargetType=\"ListBoxItem\"", xaml, StringComparison.Ordinal);
            Assert.Contains("TargetType=\"ListViewItem\"", xaml, StringComparison.Ordinal);
            Assert.Contains("TargetType=\"ComboBoxItem\"", xaml, StringComparison.Ordinal);
            Assert.Contains("TargetType=\"TreeView\"", xaml, StringComparison.Ordinal);
            Assert.Contains("TargetType=\"TreeViewItem\"", xaml, StringComparison.Ordinal);
            Assert.Contains("TargetType=\"ToolBar\"", xaml, StringComparison.Ordinal);
            Assert.Contains("TargetType=\"StatusBar\"", xaml, StringComparison.Ordinal);
            Assert.Contains("TargetType=\"Separator\"", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void AdminDesignerCoverageOverrides_ExtendThemeHooksToEditableInputsAndDataGrids()
        {
            var xaml = ReadRepositoryFile("InventoryManagementApp", "Resources", "Theme.AdminDesignerCoverageOverrides.xaml");

            Assert.Contains("AdminDesignerTextBoxTemplate", xaml, StringComparison.Ordinal);
            Assert.Contains("AdminDesignerPasswordBoxTemplate", xaml, StringComparison.Ordinal);
            Assert.Contains("AdminDesignerTextBoxBaseStyle", xaml, StringComparison.Ordinal);
            Assert.Contains("TargetType=\"TextBox\"", xaml, StringComparison.Ordinal);
            Assert.Contains("TargetType=\"RichTextBox\"", xaml, StringComparison.Ordinal);
            Assert.Contains("TargetType=\"PasswordBox\"", xaml, StringComparison.Ordinal);
            Assert.Contains("TargetType=\"DataGrid\"", xaml, StringComparison.Ordinal);
            Assert.Contains("TargetType=\"DataGridColumnHeader\"", xaml, StringComparison.Ordinal);
            Assert.Contains("TargetType=\"DataGridRow\"", xaml, StringComparison.Ordinal);
            Assert.Contains("TargetType=\"DataGridCell\"", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void ControlCustomizationOverrides_ExtendThemeHooksToSelectionAndDateControls()
        {
            var xaml = ReadRepositoryFile("InventoryManagementApp", "Resources", "Theme.ControlCustomizationOverrides.xaml");

            Assert.Contains("xmlns:primitives=\"clr-namespace:System.Windows.Controls.Primitives;assembly=PresentationFramework\"", xaml, StringComparison.Ordinal);
            Assert.Contains("TargetType=\"CheckBox\"", xaml, StringComparison.Ordinal);
            Assert.Contains("TargetType=\"RadioButton\"", xaml, StringComparison.Ordinal);
            Assert.Contains("TargetType=\"DatePicker\"", xaml, StringComparison.Ordinal);
            Assert.Contains("TargetType=\"primitives:DatePickerTextBox\"", xaml, StringComparison.Ordinal);
            Assert.Contains("TargetType=\"Calendar\"", xaml, StringComparison.Ordinal);
            Assert.Contains("TargetType=\"primitives:CalendarButton\"", xaml, StringComparison.Ordinal);
            Assert.Contains("TargetType=\"primitives:CalendarDayButton\"", xaml, StringComparison.Ordinal);
            Assert.Contains("Property=\"IsChecked\" Value=\"True\"", xaml, StringComparison.Ordinal);
            Assert.Contains("Property=\"IsKeyboardFocusWithin\" Value=\"True\"", xaml, StringComparison.Ordinal);
            Assert.Contains("Property=\"IsBlackedOut\" Value=\"True\"", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void NavigationChromeOverrides_ExtendAdminThemesToNavigationAndTextChrome()
        {
            var xaml = ReadRepositoryFile("InventoryManagementApp", "Resources", "Theme.NavigationChromeOverrides.xaml");

            Assert.Contains("TargetType=\"Frame\"", xaml, StringComparison.Ordinal);
            Assert.Contains("TargetType=\"NavigationWindow\"", xaml, StringComparison.Ordinal);
            Assert.Contains("TargetType=\"Label\"", xaml, StringComparison.Ordinal);
            Assert.Contains("TargetType=\"AccessText\"", xaml, StringComparison.Ordinal);
            Assert.Contains("TargetType=\"BulletDecorator\"", xaml, StringComparison.Ordinal);
            Assert.Contains("TargetType=\"documents:AdornerDecorator\"", xaml, StringComparison.Ordinal);
            Assert.Contains("TargetType=\"Viewbox\"", xaml, StringComparison.Ordinal);
            Assert.Contains("NavigationUIVisibility\" Value=\"Hidden\"", xaml, StringComparison.Ordinal);
            Assert.Contains("ShowsNavigationUI\" Value=\"False\"", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void NavigationChromeOverrides_UseAdminThemeTokensForBackgroundsTypographyAndBorders()
        {
            var xaml = ReadRepositoryFile("InventoryManagementApp", "Resources", "Theme.NavigationChromeOverrides.xaml");

            Assert.Contains("{DynamicResource BackgroundBrush}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource MainContentBackgroundBrush}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource TransparentSurfaceBrush}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource ForegroundBrush}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource BorderBrushAlt}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource ThemeBorderlessThickness}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource ThemeFontFamily}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource ThemeBodyFontSize}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource ThemeDisabledOpacity}", xaml, StringComparison.Ordinal);
            Assert.Contains("FocusVisualStyle\" Value=\"{StaticResource DefaultFocusVisual}\"", xaml, StringComparison.Ordinal);
            Assert.Contains("TextOptions.TextRenderingMode\" Value=\"ClearType\"", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void LayoutPresenterOverrides_ExtendAdminThemesToStructuralLayoutChrome()
        {
            var xaml = ReadRepositoryFile("InventoryManagementApp", "Resources", "Theme.LayoutPresenterOverrides.xaml");

            Assert.Contains("TargetType=\"Grid\"", xaml, StringComparison.Ordinal);
            Assert.Contains("TargetType=\"DockPanel\"", xaml, StringComparison.Ordinal);
            Assert.Contains("TargetType=\"StackPanel\"", xaml, StringComparison.Ordinal);
            Assert.Contains("TargetType=\"WrapPanel\"", xaml, StringComparison.Ordinal);
            Assert.Contains("TargetType=\"Canvas\"", xaml, StringComparison.Ordinal);
            Assert.Contains("TargetType=\"primitives:UniformGrid\"", xaml, StringComparison.Ordinal);
            Assert.Contains("TargetType=\"ContentPresenter\"", xaml, StringComparison.Ordinal);
            Assert.Contains("TargetType=\"ItemsPresenter\"", xaml, StringComparison.Ordinal);
            Assert.Contains("TargetType=\"primitives:Popup\"", xaml, StringComparison.Ordinal);
            Assert.Contains("TargetType=\"GridViewColumnHeader\"", xaml, StringComparison.Ordinal);
            Assert.Contains("TargetType=\"DocumentViewer\"", xaml, StringComparison.Ordinal);
            Assert.Contains("TargetType=\"FlowDocumentReader\"", xaml, StringComparison.Ordinal);
            Assert.Contains("TargetType=\"FlowDocumentScrollViewer\"", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void LayoutPresenterOverrides_UseAdminThemeTokensForTransparentBackgroundsAndTypography()
        {
            var xaml = ReadRepositoryFile("InventoryManagementApp", "Resources", "Theme.LayoutPresenterOverrides.xaml");

            Assert.Contains("{DynamicResource TransparentSurfaceBrush}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource BackgroundBrush}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource ForegroundBrush}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource BorderBrushAlt}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource ThemeGridLineBrush}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource ThemeControlBorderThickness}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource ThemeBorderThickness}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource ThemeFontFamily}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource ThemeBodyFontSize}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource ThemeDataGridHeaderHeight}", xaml, StringComparison.Ordinal);
            Assert.Contains("AllowsTransparency\" Value=\"True\"", xaml, StringComparison.Ordinal);
            Assert.Contains("TextOptions.TextRenderingMode\" Value=\"ClearType\"", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void SpecialSurfaceOverrides_ExtendAdminThemesToMediaDocumentResizeAndVectorChrome()
        {
            var xaml = ReadRepositoryFile("InventoryManagementApp", "Resources", "Theme.SpecialSurfaceOverrides.xaml");

            Assert.Contains("TargetType=\"Image\"", xaml, StringComparison.Ordinal);
            Assert.Contains("TargetType=\"MediaElement\"", xaml, StringComparison.Ordinal);
            Assert.Contains("TargetType=\"InkCanvas\"", xaml, StringComparison.Ordinal);
            Assert.Contains("TargetType=\"Viewport3D\"", xaml, StringComparison.Ordinal);
            Assert.Contains("TargetType=\"Rectangle\"", xaml, StringComparison.Ordinal);
            Assert.Contains("TargetType=\"Ellipse\"", xaml, StringComparison.Ordinal);
            Assert.Contains("TargetType=\"Line\"", xaml, StringComparison.Ordinal);
            Assert.Contains("TargetType=\"Path\"", xaml, StringComparison.Ordinal);
            Assert.Contains("TargetType=\"Polygon\"", xaml, StringComparison.Ordinal);
            Assert.Contains("TargetType=\"Polyline\"", xaml, StringComparison.Ordinal);
            Assert.Contains("TargetType=\"FixedPage\"", xaml, StringComparison.Ordinal);
            Assert.Contains("TargetType=\"FlowDocumentPageViewer\"", xaml, StringComparison.Ordinal);
            Assert.Contains("TargetType=\"StatusBarItem\"", xaml, StringComparison.Ordinal);
            Assert.Contains("TargetType=\"ResizeGrip\"", xaml, StringComparison.Ordinal);
            Assert.Contains("TargetType=\"primitives:Thumb\"", xaml, StringComparison.Ordinal);
            Assert.Contains("TargetType=\"primitives:Track\"", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void SpecialSurfaceOverrides_UseAdminThemeTokensForTransparencyTypographyDepthAndVectorStrokes()
        {
            var xaml = ReadRepositoryFile("InventoryManagementApp", "Resources", "Theme.SpecialSurfaceOverrides.xaml");

            Assert.Contains("{DynamicResource TransparentSurfaceBrush}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource BackgroundBrush}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource ForegroundBrush}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource BorderBrushAlt}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource AccentBrush}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource ThemeBorderThickness}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource ThemeBorderlessThickness}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource ThemeControlBorderThickness}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource ThemeShapeStrokeThickness}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource ThemeFontFamily}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource ThemeBodyFontSize}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource ThemeCaptionFontSize}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource ThemeSurfaceShadow}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource ThemeControlShadow}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource ItemHoverBrush}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource ItemSelectedBrush}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource ThemeDisabledOpacity}", xaml, StringComparison.Ordinal);
            Assert.Contains("FocusVisualStyle\" Value=\"{StaticResource DefaultFocusVisual}\"", xaml, StringComparison.Ordinal);
            Assert.Contains("TextOptions.TextRenderingMode\" Value=\"ClearType\"", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void ThemeCustomizationAndService_ExposeShapeStrokeResourceForBorderlessVectorChrome()
        {
            var resources = ReadRepositoryFile("InventoryManagementApp", "Resources", "Theme.Customization.xaml");
            var service = ReadRepositoryFile("InventoryManagementApp", "Services", "ThemeService.cs");

            Assert.Contains("x:Key=\"ThemeShapeStrokeThickness\"", resources, StringComparison.Ordinal);
            Assert.Contains("Set(resources, \"ThemeShapeStrokeThickness\", settings.BordersVisible ? settings.ControlBorderThickness : 0)", service, StringComparison.Ordinal);
        }

        [Fact]
        public void AdminDesignerCoverageOverrides_UseAdminTokensForSelectionTransparencyAndDepth()
        {
            var xaml = ReadRepositoryFile("InventoryManagementApp", "Resources", "Theme.AdminDesignerCoverageOverrides.xaml");

            Assert.Contains("{DynamicResource TransparentSurfaceBrush}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource GlassSurfaceBrush}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource GlassSurfaceAltBrush}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource TextBoxBackgroundBrush}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource ComboBoxPopupBackgroundBrush}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource ThemeShellMenuBrush}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource ThemeShellFooterBrush}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource ThemePanelCornerRadius}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource ThemeInputCornerRadius}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource ThemeBorderThickness}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource ThemeSubtleBorderThickness}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource ThemeBorderlessThickness}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource ThemeControlBorderThickness}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource ThemeSurfaceShadow}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource ThemeRaisedShadow}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource ThemeControlShadow}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource ThemeGridLineBrush}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource DataGridRowBackgroundBrush}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource DataGridAlternatingRowBackgroundBrush}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource ThemeDataGridRowHeight}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource ThemeDataGridHeaderHeight}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource ItemHoverBrush}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource ItemSelectedBrush}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource ItemSelectedForegroundBrush}", xaml, StringComparison.Ordinal);
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
