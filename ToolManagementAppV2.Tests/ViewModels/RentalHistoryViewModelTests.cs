using System;
using System.Collections.Generic;
using System.IO;
using ToolManagementAppV2.Models.Domain;
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
            var vm = new RentalHistoryViewModel(null, history);

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
            var vm = new RentalHistoryViewModel(null, history);

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
    }
}
