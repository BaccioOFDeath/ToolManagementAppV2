using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Xunit;

namespace InventoryManagementApp.Tests
{
    public class PageHeaderBandConsistencyTests
    {
        [Fact]
        public void MainPages_UseSharedFixedHeaderBandAndConfiguredBrush()
        {
            var pages = new Dictionary<string, string>
            {
                ["DashboardPage.xaml"] = "PageHeaderDashboardBrush",
                ["ItemSearchPage.xaml"] = "PageHeaderSearchBrush",
                ["ManageItemsPage.xaml"] = "PageHeaderManageItemsBrush",
                ["ManageRentalsPage.xaml"] = "PageHeaderRentalsBrush",
                ["CustomersPage.xaml"] = "PageHeaderCustomersBrush",
                ["ReservationPage.xaml"] = "PageHeaderReservationsBrush",
                ["MaintenancePage.xaml"] = "PageHeaderMaintenanceBrush",
                ["CalibrationPage.xaml"] = "PageHeaderCalibrationBrush",
                ["KitManagementPage.xaml"] = "PageHeaderKitsBrush",
                ["CategoriesPage.xaml"] = "PageHeaderCategoriesBrush",
                ["ReportsPage.xaml"] = "PageHeaderReportsBrush",
                ["ActivityLogsPage.xaml"] = "PageHeaderActivityLogsBrush",
                ["ImportExportPage.xaml"] = "PageHeaderImportExportBrush",
                ["UsersPage.xaml"] = "PageHeaderUsersBrush",
                ["SettingsPage.xaml"] = "PageHeaderSettingsBrush"
            };

            foreach (var (page, brush) in pages)
            {
                var xaml = ReadRepoFile("InventoryManagementApp", "Views", "Pages", page);

                Assert.Contains("Style=\"{StaticResource PageHeaderBand}\"", xaml, StringComparison.Ordinal);
                Assert.Contains($"Background=\"{{DynamicResource {brush}}}\"", xaml, StringComparison.Ordinal);

                var document = XDocument.Parse(xaml);
                var header = document.Descendants()
                    .Single(element => element.Name.LocalName == "Border"
                        && element.Attribute("Style")?.Value == "{StaticResource PageHeaderBand}");
                Assert.DoesNotContain(header.Descendants(), element => element.Name.LocalName == "Button");

                Assert.Contains(header.Descendants(), element => element.Name.LocalName == "WrapPanel"
                    && element.Attribute("Style")?.Value == "{StaticResource PageHeaderStatsPanel}");
                Assert.DoesNotContain(header.Descendants(), element => element.Name.LocalName == "WrapPanel"
                    && element.Attribute("Style")?.Value == "{StaticResource PageHeaderStatsPanel}"
                    && element.Attribute("HorizontalAlignment")?.Value == "Left");
            }
        }

        private static string ReadRepoFile(params string[] parts)
        {
            var directory = AppContext.BaseDirectory;
            while (!string.IsNullOrEmpty(directory))
            {
                var candidate = Path.Combine(directory, Path.Combine(parts));
                if (File.Exists(candidate))
                    return File.ReadAllText(candidate);

                directory = Directory.GetParent(directory)?.FullName;
            }

            throw new FileNotFoundException($"Could not find repository file: {Path.Combine(parts)}");
        }
    }
}
