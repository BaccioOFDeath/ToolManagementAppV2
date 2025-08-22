using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using InventoryManagementApp.Models.Domain;
using InventoryManagementApp.Interfaces;
using InventoryManagementApp.ViewModels.Rental;
using Xunit;

namespace InventoryManagementApp.Tests.ViewModels
{
    public class RentalHistoryViewModelTests
    {
        [Fact]
        public void Constructor_SetsItemDisplayName_WhenItemProvided()
        {
            var item = new ItemModel { ItemNumber = "T1", NameDescription = "Hammer" };
            var vm = new RentalHistoryViewModel(item, Enumerable.Empty<Rental>(), new StubDialogService());

            Assert.Equal("T1 - Hammer", vm.ItemDisplayName);
        }

        [Fact]
        public void Constructor_DefaultsItemDisplayName_WhenItemNull()
        {
            var vm = new RentalHistoryViewModel(null, Enumerable.Empty<Rental>(), new StubDialogService());

            Assert.Equal("Rental History", vm.ItemDisplayName);
        }

        [Fact]
        public void SearchCommand_FiltersHistory()
        {
            var history = new List<Rental>
            {
                new Rental { RentalID = 1, ItemNumber = "T1", CustomerName = "Alice", Status = "Rented", RentalDate=DateTime.Today, DueDate=DateTime.Today },
                new Rental { RentalID = 2, ItemNumber = "T2", CustomerName = "Bob", Status = "Returned", RentalDate=DateTime.Today, DueDate=DateTime.Today }
            };
            var vm = new RentalHistoryViewModel(null, history, new StubDialogService());

            vm.SearchText = "T1";
            vm.SearchCommand.Execute(null);

            Assert.Single(vm.History);
            Assert.Equal(1, vm.History[0].RentalID);
        }

        [Fact]
        public void ExportCsvCommand_CreatesFile()
        {
            var history = new List<Rental>
            {
                new Rental { RentalID = 1, ItemNumber = "T1", CustomerName = "Alice", Status = "Rented", RentalDate=DateTime.Today, DueDate=DateTime.Today }
            };
            var vm = new RentalHistoryViewModel(null, history, new StubDialogService());

            var original = Environment.CurrentDirectory;
            var tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(tempDir);
            Environment.CurrentDirectory = tempDir;
            try
            {
                vm.ExportCsvCommand.Execute(null);
                var expected = Path.Combine(tempDir, "rental_history.csv");
                Assert.True(File.Exists(expected));
            }
            finally
            {
                Environment.CurrentDirectory = original;
                if (Directory.Exists(tempDir))
                    Directory.Delete(tempDir, true);
            }
        }

        [Fact]
        public void ExportCsvCommand_ShowsError_WhenWriteFails()
        {
            var history = new List<Rental>
            {
                new Rental { RentalID = 1, ItemNumber = "T1", CustomerName = "Alice", Status = "Rented", RentalDate=DateTime.Today, DueDate=DateTime.Today }
            };
            var dialog = new StubDialogService();
            var vm = new RentalHistoryViewModel(null, history, dialog);

            var original = Environment.CurrentDirectory;
            var tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(tempDir);
            Environment.CurrentDirectory = tempDir;

            var target = Path.Combine(tempDir, "rental_history.csv");
            using (new FileStream(target, FileMode.Create, FileAccess.Read, FileShare.None))
            {
                vm.ExportCsvCommand.Execute(null);
            }

            Environment.CurrentDirectory = original;
            Directory.Delete(tempDir, true);

            Assert.True(dialog.InfoShown);
        }

        class StubDialogService : IDialogService
        {
            public bool InfoShown { get; private set; }

            public void ShowInfo(string message, string title) => InfoShown = true;

            public bool ShowConfirmation(string message, string title) => true;

            public ItemModel? ShowEditItemDialog(ItemModel item) => null;

            public void ShowItemDetails(ItemModel item) { }

            public (CustomerModel customer, DateTime dueDate)? ShowRentItemDialog(ItemModel item, IEnumerable<CustomerModel> customers) => null;

            public CustomerModel? ShowAddCustomerDialog() => null;
            public void ShowRentalsFilter(InventoryManagementApp.ViewModels.ManageRentalsViewModel viewModel) { }
            public void ShowRentalHistory(ItemModel item, System.Collections.Generic.IEnumerable<RentalModel> history) { }
            public System.Collections.Generic.Dictionary<string, string>? ShowImportMapping(System.Collections.Generic.IEnumerable<string> headers, System.Collections.Generic.IEnumerable<string> properties, System.Collections.Generic.IEnumerable<string>? requiredPropertyNames = null) => null;
            public System.Func<ItemModel, System.Collections.Generic.IEnumerable<string>>? ShowImageImportMapping() => null;
            public void ShowPrintPreview(System.Windows.Documents.FlowDocument document, string title, string description) { }
            public void ShowPrintLabelDialog() { }
        }
    }
}
