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
            var convertersIndex = xaml.IndexOf("Resources/Converters.xaml", StringComparison.Ordinal);

            Assert.True(fullCustomizationIndex >= 0, "Full customization overrides should remain loaded.");
            Assert.True(controlCustomizationIndex > fullCustomizationIndex, "Control overrides should load after full customization resources.");
            Assert.True(formMediaIndex > controlCustomizationIndex, "Form/media overrides should load after control overrides.");
            Assert.True(adminCoverageIndex > formMediaIndex, "Admin coverage overrides should remain the final theme layer.");
            Assert.True(convertersIndex > adminCoverageIndex, "Converters should remain after visual resources.");
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
        public void AdminDesignerCoverageOverrides_UseAdminTokensForSelectionTransparencyAndDepth()
        {
            var xaml = ReadRepositoryFile("InventoryManagementApp", "Resources", "Theme.AdminDesignerCoverageOverrides.xaml");

            Assert.Contains("{DynamicResource TransparentSurfaceBrush}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource GlassSurfaceBrush}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource GlassSurfaceAltBrush}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource TextBoxBackgroundBrush}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource ThemePopupSurfaceBrush}", xaml, StringComparison.Ordinal);
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
