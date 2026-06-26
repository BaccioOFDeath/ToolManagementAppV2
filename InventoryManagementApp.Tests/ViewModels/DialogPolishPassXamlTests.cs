using System;
using System.IO;
using Xunit;

namespace InventoryManagementApp.Tests.ViewModels
{
    public class DialogPolishPassXamlTests
    {
        [Fact]
        public void RentalFilterDialog_UsesPolishedFilterStructureAndPreservesCommands()
        {
            var filter = ReadRepositoryFile("InventoryManagementApp", "Views", "Windows", "RentalsFilterWindow.xaml");

            Assert.Contains("Rental Filter", filter, StringComparison.Ordinal);
            Assert.Contains("Rental Directory Criteria", filter, StringComparison.Ordinal);
            Assert.Contains("Filter Handoff", filter, StringComparison.Ordinal);
            Assert.Contains("DesktopStatusFooter", filter, StringComparison.Ordinal);
            Assert.Contains("SearchText, UpdateSourceTrigger=PropertyChanged", filter, StringComparison.Ordinal);
            Assert.Contains("FilterFrom", filter, StringComparison.Ordinal);
            Assert.Contains("FilterTo", filter, StringComparison.Ordinal);
            Assert.Contains("StatusOptions", filter, StringComparison.Ordinal);
            Assert.Contains("SelectedStatus", filter, StringComparison.Ordinal);
            Assert.Contains("ApplyFilterCommand", filter, StringComparison.Ordinal);
            Assert.Contains("ClearFilterCommand", filter, StringComparison.Ordinal);
            Assert.Contains("CloseCommand", filter, StringComparison.Ordinal);
        }

        [Fact]
        public void ReservationEditDialog_UsesPolishedRequestStructureAndPreservesBindings()
        {
            var reservation = ReadRepositoryFile("InventoryManagementApp", "Views", "Windows", "ReservationEditWindow.xaml");

            Assert.Contains("Reservation Request", reservation, StringComparison.Ordinal);
            Assert.Contains("Fulfillment Handoff", reservation, StringComparison.Ordinal);
            Assert.Contains("Reservation Details", reservation, StringComparison.Ordinal);
            Assert.Contains("DesktopStatusFooter", reservation, StringComparison.Ordinal);
            Assert.Contains("ItemSearchText, UpdateSourceTrigger=PropertyChanged", reservation, StringComparison.Ordinal);
            Assert.Contains("ClearItemSearchCommand", reservation, StringComparison.Ordinal);
            Assert.Contains("ApplySelectedItemCommand", reservation, StringComparison.Ordinal);
            Assert.Contains("SelectedSearchItem, Mode=TwoWay", reservation, StringComparison.Ordinal);
            Assert.Contains("Reservation.ItemNumber, UpdateSourceTrigger=PropertyChanged", reservation, StringComparison.Ordinal);
            Assert.Contains("Reservation.CustomerName, UpdateSourceTrigger=PropertyChanged", reservation, StringComparison.Ordinal);
            Assert.Contains("Reservation.Notes, UpdateSourceTrigger=PropertyChanged", reservation, StringComparison.Ordinal);
            Assert.Contains("SaveCancelBar", reservation, StringComparison.Ordinal);
        }

        [Fact]
        public void ItemDetailsDialog_UsesPolishedDetailStructureAndPreservesActions()
        {
            var details = ReadRepositoryFile("InventoryManagementApp", "Views", "Windows", "ItemDetailsWindow.xaml");

            Assert.Contains("Item Detail", details, StringComparison.Ordinal);
            Assert.Contains("01 Item Identity", details, StringComparison.Ordinal);
            Assert.Contains("02 Availability And Usage", details, StringComparison.Ordinal);
            Assert.Contains("03 Next Action Handoff", details, StringComparison.Ordinal);
            Assert.Contains("04 Notes And Condition", details, StringComparison.Ordinal);
            Assert.Contains("DesktopStatusFooter", details, StringComparison.Ordinal);
            Assert.Contains("PrintDetailsCommand", details, StringComparison.Ordinal);
            Assert.Contains("PlaceReservationCommand", details, StringComparison.Ordinal);
            Assert.Contains("EditCommand", details, StringComparison.Ordinal);
            Assert.Contains("RentOutCommand", details, StringComparison.Ordinal);
            Assert.Contains("ToggleCheckOutCommand", details, StringComparison.Ordinal);
            Assert.Contains("OpenCheckoutHistoryCommand", details, StringComparison.Ordinal);
            Assert.Contains("Checkout History", details, StringComparison.Ordinal);
            Assert.Contains("OpenRentalHistoryCommand", details, StringComparison.Ordinal);
            Assert.Contains("CloseCommand", details, StringComparison.Ordinal);
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
