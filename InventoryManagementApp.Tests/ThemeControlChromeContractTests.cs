using System;
using System.IO;
using Xunit;

public class ThemeControlChromeContractTests
{
    [Fact]
    public void PolishedVisualHierarchy_ThemesCommonControlChrome()
    {
        var xaml = ReadRepositoryFile("InventoryManagementApp", "Resources", "PolishedVisualHierarchy.xaml");

        Assert.Contains("ThemeButtonTemplate", xaml, StringComparison.Ordinal);
        Assert.Contains("CornerRadius=\"{DynamicResource ThemeButtonCornerRadius}\"", xaml, StringComparison.Ordinal);
        Assert.Contains("TargetType=\"TextBox\"", xaml, StringComparison.Ordinal);
        Assert.Contains("TargetType=\"PasswordBox\"", xaml, StringComparison.Ordinal);
        Assert.Contains("TargetType=\"ComboBox\"", xaml, StringComparison.Ordinal);
        Assert.Contains("TargetType=\"CheckBox\"", xaml, StringComparison.Ordinal);
        Assert.Contains("TargetType=\"ContextMenu\"", xaml, StringComparison.Ordinal);
        Assert.Contains("TargetType=\"MenuItem\"", xaml, StringComparison.Ordinal);
        Assert.Contains("TargetType=\"ListBox\"", xaml, StringComparison.Ordinal);
        Assert.Contains("TargetType=\"TabControl\"", xaml, StringComparison.Ordinal);
        Assert.Contains("TargetType=\"TabItem\"", xaml, StringComparison.Ordinal);
        Assert.Contains("TargetType=\"ProgressBar\"", xaml, StringComparison.Ordinal);
    }

    [Fact]
    public void PolishedVisualHierarchy_UsesAdminThemeTokensForCommonControls()
    {
        var xaml = ReadRepositoryFile("InventoryManagementApp", "Resources", "PolishedVisualHierarchy.xaml");

        Assert.Contains("{DynamicResource ThemeControlBorderThickness}", xaml, StringComparison.Ordinal);
        Assert.Contains("{DynamicResource ThemeControlMinHeight}", xaml, StringComparison.Ordinal);
        Assert.Contains("{DynamicResource ThemeDisabledOpacity}", xaml, StringComparison.Ordinal);
        Assert.Contains("{x:Null}", xaml, StringComparison.Ordinal);
        Assert.Contains("{DynamicResource ThemeFontFamily}", xaml, StringComparison.Ordinal);
        Assert.Contains("{DynamicResource ThemeBodyFontSize}", xaml, StringComparison.Ordinal);
        Assert.Contains("{DynamicResource ThemeDialogSurfaceBrush}", xaml, StringComparison.Ordinal);
        Assert.Contains("{DynamicResource ItemHoverBrush}", xaml, StringComparison.Ordinal);
        Assert.Contains("{DynamicResource ItemSelectedBrush}", xaml, StringComparison.Ordinal);
    }

    static string ReadRepositoryFile(params string[] relativePathParts)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory != null)
        {
            var candidate = Path.Combine(directory.FullName, Path.Combine(relativePathParts));
            if (File.Exists(candidate))
                return File.ReadAllText(candidate);

            directory = directory.Parent;
        }

        throw new FileNotFoundException("Could not locate repository file.", Path.Combine(relativePathParts));
    }
}
