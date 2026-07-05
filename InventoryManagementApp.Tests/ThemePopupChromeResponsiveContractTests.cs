using System;
using System.IO;
using Xunit;

namespace InventoryManagementApp.Tests
{
    public class ThemePopupChromeResponsiveContractTests
    {
        [Fact]
        public void App_LoadsPopupChromeAfterControlOverridesSoSharedPopupsWin()
        {
            var app = ReadRepoFile("InventoryManagementApp", "App.xaml");

            var controlOverrides = app.IndexOf("Resources/Theme.ControlCustomizationOverrides.xaml", StringComparison.Ordinal);
            var popupOverrides = app.IndexOf("Resources/Theme.PopupChromeOverrides.xaml", StringComparison.Ordinal);
            var converters = app.IndexOf("Resources/Converters.xaml", StringComparison.Ordinal);

            Assert.True(controlOverrides >= 0);
            Assert.True(popupOverrides > controlOverrides);
            Assert.True(converters > popupOverrides);
        }

        [Fact]
        public void PopupChrome_ContextMenusUseDropdownBrushAndBoundedScrollableTemplate()
        {
            var xaml = ReadPopupChrome();

            Assert.Contains("<Style TargetType=\"ContextMenu\">", xaml, StringComparison.Ordinal);
            Assert.Contains("Background\" Value=\"{DynamicResource ThemeMenuDropDownBackgroundBrush}\"", xaml, StringComparison.Ordinal);
            Assert.Contains("<Setter Property=\"MinWidth\" Value=\"180\"/>", xaml, StringComparison.Ordinal);
            Assert.Contains("<Setter Property=\"MaxWidth\" Value=\"440\"/>", xaml, StringComparison.Ordinal);
            Assert.Contains("<Setter Property=\"MaxHeight\" Value=\"420\"/>", xaml, StringComparison.Ordinal);
            Assert.Contains("<ControlTemplate TargetType=\"ContextMenu\">", xaml, StringComparison.Ordinal);
            Assert.Contains("<ScrollViewer MaxHeight=\"{TemplateBinding MaxHeight}\"", xaml, StringComparison.Ordinal);
            Assert.Contains("HorizontalScrollBarVisibility=\"Disabled\"", xaml, StringComparison.Ordinal);
            Assert.Contains("KeyboardNavigation.DirectionalNavigation=\"Contained\"", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("Background\" Value=\"{DynamicResource ThemePopupSurfaceBrush}\"/>\n        <Setter Property=\"BorderBrush\" Value=\"{DynamicResource BorderBrushAlt}\"/>\n        <Setter Property=\"BorderThickness\" Value=\"{DynamicResource ThemeBorderThickness}\"/>", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void PopupChrome_MenuAndContextMenuForceThemedSystemSelectionBrushes()
        {
            var xaml = ReadPopupChrome();

            Assert.Contains("{x:Static SystemColors.HighlightBrushKey}", xaml, StringComparison.Ordinal);
            Assert.Contains("{x:Static SystemColors.HighlightTextBrushKey}", xaml, StringComparison.Ordinal);
            Assert.Contains("{x:Static SystemColors.ControlTextBrushKey}", xaml, StringComparison.Ordinal);
            Assert.Contains("{x:Static SystemColors.GrayTextBrushKey}", xaml, StringComparison.Ordinal);
            Assert.Contains("Color=\"{DynamicResource Col.Accent}\"", xaml, StringComparison.Ordinal);
            Assert.Contains("Color=\"{DynamicResource Col.OnAccent}\"", xaml, StringComparison.Ordinal);
            Assert.Contains("Color=\"{DynamicResource Col.Fg}\"", xaml, StringComparison.Ordinal);
            Assert.Contains("Color=\"{DynamicResource Col.FgMuted}\"", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void PopupChrome_MenuItemsTrimLongLabelsAndKeepDisabledTextReadable()
        {
            var xaml = ReadPopupChrome();

            Assert.Contains("<Style x:Key=\"ThemePopupMenuItemStyle\" TargetType=\"MenuItem\">", xaml, StringComparison.Ordinal);
            Assert.Contains("<Setter Property=\"MinWidth\" Value=\"140\"/>", xaml, StringComparison.Ordinal);
            Assert.Contains("<Setter Property=\"MaxWidth\" Value=\"420\"/>", xaml, StringComparison.Ordinal);
            Assert.Contains("<Setter Property=\"TextBlock.TextWrapping\" Value=\"NoWrap\"/>", xaml, StringComparison.Ordinal);
            Assert.Contains("<Setter Property=\"TextBlock.TextTrimming\" Value=\"CharacterEllipsis\"/>", xaml, StringComparison.Ordinal);
            Assert.Contains("<Trigger Property=\"IsHighlighted\" Value=\"True\">", xaml, StringComparison.Ordinal);
            Assert.Contains("<Setter Property=\"Foreground\" Value=\"{DynamicResource ItemHoverForegroundBrush}\"/>", xaml, StringComparison.Ordinal);
            Assert.Contains("<Trigger Property=\"IsSubmenuOpen\" Value=\"True\">", xaml, StringComparison.Ordinal);
            Assert.Contains("<Setter Property=\"Foreground\" Value=\"{DynamicResource ForegroundMutedBrush}\"/>", xaml, StringComparison.Ordinal);
            Assert.Contains("<Setter Property=\"Opacity\" Value=\"{DynamicResource ThemeDisabledOpacity}\"/>", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void PopupChrome_ComboBoxesUseVirtualizedBoundedDropDowns()
        {
            var xaml = ReadPopupChrome();

            Assert.Contains("<Style x:Key=\"ThemeResponsiveComboBoxStyle\" TargetType=\"ComboBox\" BasedOn=\"{StaticResource ThemedComboBoxStyle}\">", xaml, StringComparison.Ordinal);
            Assert.Contains("<Setter Property=\"ItemContainerStyle\" Value=\"{StaticResource ThemePopupComboBoxItemStyle}\"/>", xaml, StringComparison.Ordinal);
            Assert.Contains("<Setter Property=\"MaxDropDownHeight\" Value=\"320\"/>", xaml, StringComparison.Ordinal);
            Assert.Contains("<Setter Property=\"VirtualizingPanel.IsVirtualizing\" Value=\"True\"/>", xaml, StringComparison.Ordinal);
            Assert.Contains("<Setter Property=\"VirtualizingPanel.VirtualizationMode\" Value=\"Recycling\"/>", xaml, StringComparison.Ordinal);
            Assert.Contains("<VirtualizingStackPanel/>", xaml, StringComparison.Ordinal);
            Assert.Contains("MaxWidth=\"420\"", xaml, StringComparison.Ordinal);
            Assert.Contains("MaxHeight=\"{TemplateBinding MaxDropDownHeight}\"", xaml, StringComparison.Ordinal);
            Assert.Contains("CanContentScroll=\"True\"", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("<ScrollViewer>\n                                    <ItemsPresenter", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void PopupChrome_ComboBoxItemsTrimLongValuesAndUseFocusReadableColors()
        {
            var xaml = ReadPopupChrome();

            Assert.Contains("<Style x:Key=\"ThemePopupComboBoxItemStyle\" TargetType=\"ComboBoxItem\" BasedOn=\"{StaticResource DropdownItemStyle}\">", xaml, StringComparison.Ordinal);
            Assert.Contains("<Setter Property=\"MaxWidth\" Value=\"420\"/>", xaml, StringComparison.Ordinal);
            Assert.Contains("<Setter Property=\"HorizontalContentAlignment\" Value=\"Stretch\"/>", xaml, StringComparison.Ordinal);
            Assert.Contains("<Setter Property=\"TextBlock.TextWrapping\" Value=\"NoWrap\"/>", xaml, StringComparison.Ordinal);
            Assert.Contains("<Setter Property=\"TextBlock.TextTrimming\" Value=\"CharacterEllipsis\"/>", xaml, StringComparison.Ordinal);
            Assert.Contains("<Trigger Property=\"IsKeyboardFocusWithin\" Value=\"True\">", xaml, StringComparison.Ordinal);
            Assert.Contains("<Setter Property=\"Foreground\" Value=\"{DynamicResource ItemHoverForegroundBrush}\"/>", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void PopupChrome_ToolTipsAndStatusBarsUseLayoutRoundingAndProfessionalTextBounds()
        {
            var xaml = ReadPopupChrome();

            Assert.Contains("<Style TargetType=\"ToolTip\">", xaml, StringComparison.Ordinal);
            Assert.Contains("<Setter Property=\"MaxWidth\" Value=\"420\"/>", xaml, StringComparison.Ordinal);
            Assert.Contains("<Setter Property=\"TextBlock.TextWrapping\" Value=\"Wrap\"/>", xaml, StringComparison.Ordinal);
            Assert.Contains("<Setter Property=\"SnapsToDevicePixels\" Value=\"True\"/>", xaml, StringComparison.Ordinal);
            Assert.Contains("<Setter Property=\"UseLayoutRounding\" Value=\"True\"/>", xaml, StringComparison.Ordinal);
            Assert.Contains("<Style TargetType=\"StatusBar\">", xaml, StringComparison.Ordinal);
            Assert.Contains("<Style TargetType=\"StatusBarItem\">", xaml, StringComparison.Ordinal);
            Assert.Contains("<Setter Property=\"TextBlock.TextTrimming\" Value=\"CharacterEllipsis\"/>", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void ThemeService_KeepsMenuDropdownOpacityIndependentForMenusAndComboBoxes()
        {
            var service = ReadRepoFile("InventoryManagementApp", "Services", "ThemeService.cs");
            var tests = ReadRepoFile("InventoryManagementApp.Tests", "ThemeServiceTests.cs");

            Assert.Contains("Set(resources, \"ThemeMenuDropDownBackgroundBrush\", CreateBrush(settings.SurfaceAltColor, settings.MenuDropDownOpacity));", service, StringComparison.Ordinal);
            Assert.Contains("Set(resources, \"ComboBoxPopupBackgroundBrush\", CreateBrush(settings.SurfaceAltColor, settings.MenuDropDownOpacity));", service, StringComparison.Ordinal);
            Assert.Contains("ApplyCustomTheme_MenuDropdownOpacityIsIndependentFromMainSurfaces", tests, StringComparison.Ordinal);
            Assert.Contains("Assert.Equal(0xEB, ((SolidColorBrush)app.Resources[\"ThemeMenuDropDownBackgroundBrush\"]).Color.A);", tests, StringComparison.Ordinal);
            Assert.Contains("Assert.Equal(0xEB, ((SolidColorBrush)app.Resources[\"ComboBoxPopupBackgroundBrush\"]).Color.A);", tests, StringComparison.Ordinal);
        }

        private static string ReadPopupChrome()
            => ReadRepoFile("InventoryManagementApp", "Resources", "Theme.PopupChromeOverrides.xaml");

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
