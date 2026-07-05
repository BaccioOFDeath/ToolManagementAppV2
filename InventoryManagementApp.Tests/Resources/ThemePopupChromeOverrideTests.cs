using System;
using System.IO;
using Xunit;

namespace InventoryManagementApp.Tests.Resources
{
    public class ThemePopupChromeOverrideTests
    {
        [Fact]
        public void App_LoadsPopupChromeOverridesAfterRangeScrollLayer()
        {
            var xaml = ReadRepositoryFile("InventoryManagementApp", "App.xaml");

            var rangeScrollIndex = xaml.IndexOf("Resources/Theme.RangeScrollChromeOverrides.xaml", StringComparison.Ordinal);
            var popupIndex = xaml.IndexOf("Resources/Theme.PopupChromeOverrides.xaml", StringComparison.Ordinal);
            var convertersIndex = xaml.IndexOf("Resources/Converters.xaml", StringComparison.Ordinal);

            Assert.True(rangeScrollIndex >= 0, "Range and scroll chrome overrides should remain loaded.");
            Assert.True(popupIndex > rangeScrollIndex, "Popup chrome overrides should load after the range/scroll layer.");
            Assert.True(convertersIndex > popupIndex, "Converters should remain after the final visual override layer.");
        }

        [Fact]
        public void PopupChromeOverrides_ExtendAdminThemesToPopupAndMenuSurfaces()
        {
            var xaml = ReadRepositoryFile("InventoryManagementApp", "Resources", "Theme.PopupChromeOverrides.xaml");

            Assert.Contains("TargetType=\"ContextMenu\"", xaml, StringComparison.Ordinal);
            Assert.Contains("TargetType=\"Menu\"", xaml, StringComparison.Ordinal);
            Assert.Contains("TargetType=\"MenuItem\"", xaml, StringComparison.Ordinal);
            Assert.Contains("TargetType=\"ComboBox\"", xaml, StringComparison.Ordinal);
            Assert.Contains("TargetType=\"ToolTip\"", xaml, StringComparison.Ordinal);
            Assert.Contains("TargetType=\"StatusBar\"", xaml, StringComparison.Ordinal);
            Assert.Contains("TargetType=\"StatusBarItem\"", xaml, StringComparison.Ordinal);
            Assert.Contains("TargetType=\"Separator\"", xaml, StringComparison.Ordinal);
            Assert.Contains("ThemePopupMenuItemStyle", xaml, StringComparison.Ordinal);
            Assert.Contains("ThemePopupSeparatorStyle", xaml, StringComparison.Ordinal);
            Assert.Contains("ThemeResponsiveComboBoxStyle", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void PopupChromeOverrides_DoNotApplyMenuItemContainerStyleToSeparators()
        {
            var xaml = ReadRepositoryFile("InventoryManagementApp", "Resources", "Theme.PopupChromeOverrides.xaml");

            Assert.DoesNotContain("Property=\"ItemContainerStyle\" Value=\"{StaticResource ThemePopupMenuItemStyle}\"", xaml, StringComparison.Ordinal);
            Assert.Contains("<Style TargetType=\"MenuItem\" BasedOn=\"{StaticResource ThemePopupMenuItemStyle}\"/>", xaml, StringComparison.Ordinal);
            Assert.Contains("<Style TargetType=\"Separator\" BasedOn=\"{StaticResource ThemePopupSeparatorStyle}\"/>", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void PopupChromeOverrides_BoundLongDropdownsAndMenusForResponsiveOpening()
        {
            var xaml = ReadRepositoryFile("InventoryManagementApp", "Resources", "Theme.PopupChromeOverrides.xaml");

            Assert.Contains("MaxDropDownHeight\" Value=\"320\"", xaml, StringComparison.Ordinal);
            Assert.Contains("MaxHeight=\"{TemplateBinding MaxDropDownHeight}\"", xaml, StringComparison.Ordinal);
            Assert.Contains("MinWidth=\"{Binding ActualWidth, RelativeSource={RelativeSource TemplatedParent}}\"", xaml, StringComparison.Ordinal);
            Assert.Contains("VerticalScrollBarVisibility=\"Auto\"", xaml, StringComparison.Ordinal);
            Assert.Contains("HorizontalScrollBarVisibility=\"Disabled\"", xaml, StringComparison.Ordinal);
            Assert.Contains("CanContentScroll=\"True\"", xaml, StringComparison.Ordinal);
            Assert.Contains("KeyboardNavigation.DirectionalNavigation=\"Contained\"", xaml, StringComparison.Ordinal);
            Assert.Contains("<Setter Property=\"MaxHeight\" Value=\"420\"/>", xaml, StringComparison.Ordinal);
            Assert.Contains("<Setter Property=\"MaxWidth\" Value=\"420\"/>", xaml, StringComparison.Ordinal);
            Assert.Contains("TextBlock.TextWrapping\" Value=\"Wrap\"", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void PopupChromeOverrides_RecycleDropdownItemsForLargeLists()
        {
            var xaml = ReadRepositoryFile("InventoryManagementApp", "Resources", "Theme.PopupChromeOverrides.xaml");

            Assert.Contains("<VirtualizingStackPanel/>", xaml, StringComparison.Ordinal);
            Assert.Contains("VirtualizingPanel.IsVirtualizing\" Value=\"True\"", xaml, StringComparison.Ordinal);
            Assert.Contains("VirtualizingPanel.VirtualizationMode\" Value=\"Recycling\"", xaml, StringComparison.Ordinal);
            Assert.Contains("VirtualizingPanel.IsVirtualizing=\"True\"", xaml, StringComparison.Ordinal);
            Assert.Contains("VirtualizingPanel.VirtualizationMode=\"Recycling\"", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void PopupChromeOverrides_PreserveSharedComboBoxTemplateContracts()
        {
            var xaml = ReadRepositoryFile("InventoryManagementApp", "Resources", "Theme.PopupChromeOverrides.xaml");

            Assert.Contains("BasedOn=\"{StaticResource ThemedComboBoxStyle}\"", xaml, StringComparison.Ordinal);
            Assert.Contains("Name=\"ToggleButton\"", xaml, StringComparison.Ordinal);
            Assert.Contains("Name=\"ContentSite\"", xaml, StringComparison.Ordinal);
            Assert.Contains("Name=\"Popup\"", xaml, StringComparison.Ordinal);
            Assert.Contains("SelectionBoxItem", xaml, StringComparison.Ordinal);
            Assert.Contains("IsDropDownOpen", xaml, StringComparison.Ordinal);
            Assert.Contains("ItemTemplateSelector", xaml, StringComparison.Ordinal);
            Assert.Contains("Width=\"8\"", xaml, StringComparison.Ordinal);
            Assert.Contains("Height=\"4\"", xaml, StringComparison.Ordinal);
            Assert.Contains("Stretch=\"Uniform\"", xaml, StringComparison.Ordinal);
            Assert.Contains("Opacity=\"0.72\"", xaml, StringComparison.Ordinal);
            Assert.Contains("DisabledForegroundBrush", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void PopupChromeOverrides_UseAdminThemeTokensForTransparencyDepthAndInteraction()
        {
            var xaml = ReadRepositoryFile("InventoryManagementApp", "Resources", "Theme.PopupChromeOverrides.xaml");

            Assert.Contains("{DynamicResource ThemePopupSurfaceBrush}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource ThemeShellMenuBrush}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource ThemeShellFooterBrush}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource TransparentSurfaceBrush}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource ThemeTransparentBrush}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource BorderBrushAlt}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource ThemeBorderThickness}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource ThemeBorderlessThickness}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource ThemeSubtleBorderThickness}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource ThemeFontFamily}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource ThemeBodyFontSize}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource ThemeCaptionFontSize}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource ThemeControlMinHeight}", xaml, StringComparison.Ordinal);
            Assert.Contains("{x:Null}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource ItemHoverBrush}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource ItemSelectedBrush}", xaml, StringComparison.Ordinal);
            Assert.Contains("{DynamicResource ThemeDisabledOpacity}", xaml, StringComparison.Ordinal);
            Assert.Contains("FocusVisualStyle\" Value=\"{StaticResource DefaultFocusVisual}\"", xaml, StringComparison.Ordinal);
            Assert.Contains("HasDropShadow\" Value=\"False\"", xaml, StringComparison.Ordinal);
        }

        private static string ReadRepositoryFile(params string[] relativePathParts)
        {
            var directory = new DirectoryInfo(AppContext.BaseDirectory);
            while (directory != null && !File.Exists(Path.Combine(directory.FullName, "InventoryManagementApp.sln")))
                directory = directory.Parent;

            Assert.NotNull(directory);
            var path = Path.Combine(directory!.FullName, Path.Combine(relativePathParts));
            Assert.True(File.Exists(path), $"Expected repository file at {path}");
            return NormalizeLineEndings(File.ReadAllText(path));
        }
        static string NormalizeLineEndings(string text)
            => text.Replace("\r\n", "\n");

    }
}
