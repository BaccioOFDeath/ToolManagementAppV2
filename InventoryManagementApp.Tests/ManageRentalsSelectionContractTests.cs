using System;
using System.IO;
using Xunit;

namespace InventoryManagementApp.Tests
{
    public class ManageRentalsSelectionContractTests
    {
        [Fact]
        public void RentalsReloadRestoresSelectionFromFreshFilteredRows()
        {
            var source = ReadRepoFile("InventoryManagementApp", "ViewModels", "ManageRentalsViewModel.cs");

            Assert.Contains("var selectedRentalId = SelectedRental?.RentalID;", source, StringComparison.Ordinal);
            Assert.Contains("ApplyFilter(selectedRentalId);", source, StringComparison.Ordinal);
            Assert.Contains("void ApplyFilter() => ApplyFilter(SelectedRental?.RentalID);", source, StringComparison.Ordinal);
            Assert.Contains("void ApplyFilter(int? selectedRentalId)", source, StringComparison.Ordinal);
            Assert.Contains("Rentals.ReplaceRange(filtered.ToList());", source, StringComparison.Ordinal);
            Assert.Contains("RestoreSelectedRental(selectedRentalId);", source, StringComparison.Ordinal);
            Assert.Contains("void RestoreSelectedRental(int? selectedRentalId)", source, StringComparison.Ordinal);
            Assert.Contains("SelectedRental = Rentals.FirstOrDefault(r => r.RentalID == selectedRentalId.Value);", source, StringComparison.Ordinal);
        }

        [Fact]
        public void MissingSelectionAfterFilterClearsRentalActions()
        {
            var source = ReadRepoFile("InventoryManagementApp", "ViewModels", "ManageRentalsViewModel.cs");

            Assert.Contains("if (!selectedRentalId.HasValue)", source, StringComparison.Ordinal);
            Assert.Contains("if (SelectedRental != null && !Rentals.Contains(SelectedRental))", source, StringComparison.Ordinal);
            Assert.Contains("SelectedRental = null;", source, StringComparison.Ordinal);
            Assert.Contains("bool CanReturnSelectedRental() => SelectedRental != null && IsRentalActive(SelectedRental);", source, StringComparison.Ordinal);
            Assert.Contains("bool CanPlaceRequestForSelectedRental() => SelectedRental != null && IsRentalActive(SelectedRental);", source, StringComparison.Ordinal);
        }

        [Fact]
        public void RentalGridRightClickSelectionUsesSharedSafeTreeTraversal()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "ManageRentalsPage.xaml.cs");
            var helper = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "GridContextMenuSelection.cs");

            Assert.Contains("var row = GridContextMenuSelection.SelectRow(sender, e);", source, StringComparison.Ordinal);
            Assert.Contains("GridContextMenuSelection.FindAncestor<System.Windows.Controls.DataGrid>(focusedElement)", source, StringComparison.Ordinal);
            Assert.DoesNotContain("VisualTreeHelper.GetParent", source, StringComparison.Ordinal);
            Assert.DoesNotContain("private static DependencyObject? GetParent", source, StringComparison.Ordinal);
            Assert.Contains("return VisualTreeHelper.GetParent(current)", helper, StringComparison.Ordinal);
            Assert.Contains("?? LogicalTreeHelper.GetParent(current);", helper, StringComparison.Ordinal);
            Assert.Contains("catch (InvalidOperationException)", helper, StringComparison.Ordinal);
            Assert.Contains("return LogicalTreeHelper.GetParent(current);", helper, StringComparison.Ordinal);
        }

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