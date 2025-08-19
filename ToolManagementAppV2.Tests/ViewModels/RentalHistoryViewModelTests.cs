using System;
using System.Collections.Generic;
using System.IO;
using ToolManagementAppV2.Models.Domain;
using ToolManagementAppV2.Interfaces;
using ToolManagementAppV2.ViewModels.Rental;
using Xunit;

namespace ToolManagementAppV2.Tests.ViewModels
{
    public class RentalHistoryViewModelTests
    {
        [Fact]
        public void SearchCommand_FiltersHistory()
        {
            var history = new List<Rental>
            {
                new Rental { RentalID = 1, ToolNumber = "T1", CustomerName = "Alice", Status = "Rented", RentalDate=DateTime.Today, DueDate=DateTime.Today },
                new Rental { RentalID = 2, ToolNumber = "T2", CustomerName = "Bob", Status = "Returned", RentalDate=DateTime.Today, DueDate=DateTime.Today }
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
                new Rental { RentalID = 1, ToolNumber = "T1", CustomerName = "Alice", Status = "Rented", RentalDate=DateTime.Today, DueDate=DateTime.Today }
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
                new Rental { RentalID = 1, ToolNumber = "T1", CustomerName = "Alice", Status = "Rented", RentalDate=DateTime.Today, DueDate=DateTime.Today }
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

            public ToolModel? ShowEditToolDialog(ToolModel tool) => null;

            public void ShowToolDetails(ToolModel tool) { }

            public (CustomerModel customer, DateTime dueDate)? ShowRentToolDialog(ToolModel tool, IEnumerable<CustomerModel> customers) => null;

            public CustomerModel? ShowAddCustomerDialog() => null;
            public void ShowRentalsFilter(ToolManagementAppV2.ViewModels.ManageRentalsViewModel viewModel) { }
            public void ShowRentalHistory(ToolModel tool, System.Collections.Generic.IEnumerable<RentalModel> history) { }
            public System.Collections.Generic.Dictionary<string, string>? ShowImportMapping(System.Collections.Generic.IEnumerable<string> headers, System.Collections.Generic.IEnumerable<string> properties) => null;
            public System.Func<ToolModel, System.Collections.Generic.IEnumerable<string>>? ShowImageImportMapping() => null;
            public void ShowPrintPreview(System.Windows.Documents.FlowDocument document, string title, string description) { }
            public void ShowPrintLabelDialog() { }
        }
    }
}
