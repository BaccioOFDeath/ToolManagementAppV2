using System;
using System.IO;
using System.Linq;
using Xunit;

namespace InventoryManagementApp.Tests;

public class SDEuropeanBrandingContractTests
{
    [Fact]
    public void App_LoadsBrandLayerAfterGenericThemeOverrides()
    {
        var app = ReadRepositoryFile("InventoryManagementApp", "App.xaml");
        var genericThemeIndex = app.IndexOf("Resources/Theme.AdornerValidationOverrides.xaml", StringComparison.Ordinal);
        var brandIndex = app.IndexOf("Resources/Brand.SDEuropean.xaml", StringComparison.Ordinal);

        Assert.True(genericThemeIndex >= 0);
        Assert.True(brandIndex > genericThemeIndex);
        Assert.Contains("Resources/BrandSymbol.png", app, StringComparison.Ordinal);
    }

    [Fact]
    public void BrandLayer_UsesApprovedMarksTypographyAndCoreColours()
    {
        var brand = ReadRepositoryFile("InventoryManagementApp", "Resources", "Brand.SDEuropean.xaml");

        Assert.Contains("#FF010101", brand, StringComparison.Ordinal);
        Assert.Contains("#FFFFC326", brand, StringComparison.Ordinal);
        Assert.Contains("#FFD8D8D8", brand, StringComparison.Ordinal);
        Assert.Contains("#League Spartan", brand, StringComparison.Ordinal);
        Assert.Contains("#Source Sans 3", brand, StringComparison.Ordinal);
        Assert.Contains("x:Key=\"PageHeaderBand\"", brand, StringComparison.Ordinal);
        Assert.Contains("BorderThickness\" Value=\"0,0,0,4\"", brand, StringComparison.Ordinal);
    }

    [Fact]
    public void SharedTableAndTabChrome_PreservesAvailableWidthAndReadableHeaders()
    {
        var brand = ReadRepositoryFile("InventoryManagementApp", "Resources", "Brand.SDEuropean.xaml");
        var controls = ReadRepositoryFile("InventoryManagementApp", "Resources", "Theme.ControlCustomizationOverrides.xaml");
        var grids = ReadRepositoryFile("InventoryManagementApp", "Resources", "Theme.AdminDesignerCoverageOverrides.xaml");
        var search = ReadRepositoryFile("InventoryManagementApp", "Views", "Pages", "SearchBar.xaml");

        Assert.Contains("<Setter Property=\"HorizontalContentAlignment\" Value=\"Stretch\"/>", brand, StringComparison.Ordinal);
        Assert.Contains("<Setter Property=\"VerticalContentAlignment\" Value=\"Stretch\"/>", brand, StringComparison.Ordinal);
        Assert.Contains("HorizontalAlignment=\"Center\"", controls, StringComparison.Ordinal);
        Assert.Contains("<Setter Property=\"ColumnWidth\" Value=\"*\"/>", grids, StringComparison.Ordinal);
        Assert.Contains("x:Name=\"SearchTextBox\"", search, StringComparison.Ordinal);
        Assert.Contains("MinWidth=\"80\"", search, StringComparison.Ordinal);
    }

    [Fact]
    public void ShellAndLogin_UseApprovedLogoAssetsAndBrandBands()
    {
        var shell = ReadRepositoryFile("InventoryManagementApp", "MainWindow.xaml");
        var login = ReadRepositoryFile("InventoryManagementApp", "Views", "Windows", "LoginWindow.xaml");

        Assert.Contains("Resources/BrandLogoWhite.png", shell, StringComparison.Ordinal);
        Assert.Contains("BrandBlackBrush", shell, StringComparison.Ordinal);
        Assert.Contains("BrandYellowBrush", shell, StringComparison.Ordinal);
        Assert.Contains("Resources/BrandLogoWhite.png", login, StringComparison.Ordinal);
        Assert.Contains("FAMILY | DRIVEN | INNOVATION | QUALITY", login, StringComparison.Ordinal);
        Assert.Contains("Your European vehicle experts", login, StringComparison.Ordinal);
    }

    private static string ReadRepositoryFile(params string[] relativeSegments)
    {
        var root = FindRepositoryRoot();
        return File.ReadAllText(Path.Combine(new[] { root }.Concat(relativeSegments).ToArray()));
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "InventoryManagementApp.sln")))
                return directory.FullName;

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate repository root.");
    }
}
