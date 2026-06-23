using System;
using System.Collections.Generic;
using System.IO;
using Xunit;

namespace InventoryManagementApp.Tests.Views
{
    public class GridContextMenuSelectionContractTests
    {
        [Fact]
        public void SharedGridContextMenuSelectionUsesNonThrowingVisualLogicalAndFrameworkTraversal()
        {
            var helper = ReadRepositoryFile("InventoryManagementApp", "Views", "Pages", "GridContextMenuSelection.cs");

            Assert.Contains("internal static class GridContextMenuSelection", helper, StringComparison.Ordinal);
            Assert.Contains("public static DataGridRow? SelectRow(object sender, MouseButtonEventArgs e)", helper, StringComparison.Ordinal);
            Assert.Contains("sender as DataGridRow ?? FindAncestor<DataGridRow>(e.OriginalSource as DependencyObject)", helper, StringComparison.Ordinal);
            Assert.Contains("FindAncestor<DataGrid>(row)", helper, StringComparison.Ordinal);
            Assert.Contains("return TryGetVisualParent(current)", helper, StringComparison.Ordinal);
            Assert.Contains("?? TryGetLogicalParent(current)", helper, StringComparison.Ordinal);
            Assert.Contains("?? TryGetFrameworkParent(current);", helper, StringComparison.Ordinal);
            Assert.Contains("private static DependencyObject? TryGetVisualParent(DependencyObject current)", helper, StringComparison.Ordinal);
            Assert.Contains("private static DependencyObject? TryGetLogicalParent(DependencyObject current)", helper, StringComparison.Ordinal);
            Assert.Equal(2, CountOccurrences(helper, "catch (InvalidOperationException)"));
            Assert.Contains("FrameworkElement element => element.Parent", helper, StringComparison.Ordinal);
            Assert.Contains("FrameworkContentElement contentElement => contentElement.Parent", helper, StringComparison.Ordinal);
            Assert.DoesNotContain("return LogicalTreeHelper.GetParent(current);", helper, StringComparison.Ordinal);
        }

        [Fact]
        public void OperationalGridPagesUseSharedContextMenuSelectionHelper()
        {
            var pagePaths = new[]
            {
                new[] { "InventoryManagementApp", "Views", "Pages", "ManageItemsPage.xaml.cs" },
                new[] { "InventoryManagementApp", "Views", "Pages", "ItemSearchPage.xaml.cs" },
                new[] { "InventoryManagementApp", "Views", "Pages", "ManageRentalsPage.xaml.cs" },
                new[] { "InventoryManagementApp", "Views", "Pages", "DashboardPage.xaml.cs" },
                new[] { "InventoryManagementApp", "Views", "Pages", "ReservationPage.xaml.cs" },
                new[] { "InventoryManagementApp", "Views", "Pages", "KitManagementPage.xaml.cs" },
                new[] { "InventoryManagementApp", "Views", "Pages", "CategoriesPage.xaml.cs" },
                new[] { "InventoryManagementApp", "Views", "Pages", "ReportsPage.xaml.cs" },
                new[] { "InventoryManagementApp", "Views", "Pages", "UsersPage.xaml.cs" },
                new[] { "InventoryManagementApp", "Views", "Pages", "CustomersPage.xaml.cs" },
                new[] { "InventoryManagementApp", "Views", "Pages", "MaintenancePage.xaml.cs" },
                new[] { "InventoryManagementApp", "Views", "Pages", "CalibrationPage.xaml.cs" }
            };

            foreach (var pagePath in pagePaths)
            {
                var source = ReadRepositoryFile(pagePath);
                Assert.Contains("GridContextMenuSelection.SelectRow(sender, e)", source, StringComparison.Ordinal);
            }
        }

        [Fact]
        public void UpdatedGridPagesDoNotOwnDirectContextMenuParentWalks()
        {
            var pagePaths = new Dictionary<string[], string[]>
            {
                [new[] { "InventoryManagementApp", "Views", "Pages", "ItemSearchPage.xaml.cs" }] = new[] { "private static T? FindAncestor", "private static DependencyObject? GetParent" },
                [new[] { "InventoryManagementApp", "Views", "Pages", "ManageRentalsPage.xaml.cs" }] = new[] { "private static T? FindAncestor", "private static DependencyObject? GetParent" },
                [new[] { "InventoryManagementApp", "Views", "Pages", "DashboardPage.xaml.cs" }] = new[] { "current = VisualTreeHelper.GetParent(current);" },
                [new[] { "InventoryManagementApp", "Views", "Pages", "KitManagementPage.xaml.cs" }] = new[] { "private static T? FindParent", "VisualTreeHelper.GetParent(child)" },
                [new[] { "InventoryManagementApp", "Views", "Pages", "ReportsPage.xaml.cs" }] = new[] { "private static T? FindParent", "VisualTreeHelper.GetParent(child)" },
                [new[] { "InventoryManagementApp", "Views", "Pages", "UsersPage.xaml.cs" }] = new[] { "if (sender is DataGridRow row", "row.IsSelected = true;", "e.Handled = true;" }
            };

            foreach (var entry in pagePaths)
            {
                var source = ReadRepositoryFile(entry.Key);
                foreach (var removedPattern in entry.Value)
                    Assert.DoesNotContain(removedPattern, source, StringComparison.Ordinal);
            }
        }

        private static int CountOccurrences(string source, string value)
        {
            var count = 0;
            var index = 0;

            while ((index = source.IndexOf(value, index, StringComparison.Ordinal)) >= 0)
            {
                count++;
                index += value.Length;
            }

            return count;
        }

        private static string ReadRepositoryFile(params string[] relativePath)
        {
            var directory = new DirectoryInfo(AppContext.BaseDirectory);
            while (directory != null)
            {
                var candidate = Path.Combine(directory.FullName, Path.Combine(relativePath));
                if (File.Exists(candidate))
                    return File.ReadAllText(candidate);

                directory = directory.Parent;
            }

            throw new FileNotFoundException("Could not locate repository file.", Path.Combine(relativePath));
        }
    }
}
