using System.IO;
using System.Linq;
using ToolManagementAppV2.Models.Domain;
using ToolManagementAppV2.Services.Core;
using ToolManagementAppV2.Services.Tools;
using ToolManagementAppV2.Services.Customers;
using ToolManagementAppV2.Services.Rentals;
using ToolManagementAppV2.Interfaces;
using ToolManagementAppV2.ViewModels;
using Xunit;

namespace ToolManagementAppV2.Tests.ViewModels
{
    public class ToolManagementViewModelTests
    {
        [Fact]
        public void SearchCommand_FiltersToolsBySearchTerm()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var db = new DatabaseService(dbPath);
                IToolService toolService = new ToolService(db);
                var customerService = new CustomerService(db);
                var rentalService = new RentalService(db);
                var vm = new ToolManagementViewModel(toolService, customerService, rentalService);
                toolService.AddTool(new Tool { ToolNumber = "T1", NameDescription = "Hammer" });
                toolService.AddTool(new Tool { ToolNumber = "T2", NameDescription = "Saw" });
                vm.SearchTerm = "Ham";
                vm.SearchCommand.Execute(null);
                Assert.Single(vm.SearchResults);
                Assert.Equal("Hammer", vm.SearchResults.First().NameDescription);
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }

        [Fact]
        public void SearchCommand_SortsResultsIntoCategories()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var db = new DatabaseService(dbPath);
                IToolService toolService = new ToolService(db);
                var customerService = new CustomerService(db);
                var rentalService = new RentalService(db);
                var vm = new ToolManagementViewModel(toolService, customerService, rentalService);
                toolService.AddTool(new Tool { ToolNumber = "T1", NameDescription = "Hammer" });
                toolService.AddTool(new Tool { ToolNumber = "T2", NameDescription = "Cordless Drill" });
                vm.SearchTerm = string.Empty;
                vm.SearchCommand.Execute(null);
                Assert.Single(vm.HandTools);
                Assert.Single(vm.PowerTools);
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }

        [Fact]
        public void NewToolCommand_PersistsNewToolValues()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var db = new DatabaseService(dbPath);
                IToolService toolService = new ToolService(db);
                var customerService = new CustomerService(db);
                var rentalService = new RentalService(db);
                var vm = new ToolManagementViewModel(toolService, customerService, rentalService);
                vm.NewTool.ToolNumber = "TN1";
                vm.NewTool.NameDescription = "Hammer";
                vm.NewTool.PartNumber = "PN1";
                vm.NewTool.Brand = "BrandA";
                vm.NewTool.Location = "Shelf";
                vm.NewTool.QuantityOnHand = 5;
                vm.NewTool.Supplier = "ABC";
                vm.NewTool.Notes = "Note";
                vm.NewToolCommand.Execute(null);
                var tools = toolService.GetAllTools();
                Assert.Single(tools);
                var tool = tools.First();
                Assert.Equal("TN1", tool.ToolNumber);
                Assert.Equal("Hammer", tool.NameDescription);
                Assert.Equal("PN1", tool.PartNumber);
                Assert.Equal("BrandA", tool.Brand);
                Assert.Equal("Shelf", tool.Location);
                Assert.Equal(5, tool.QuantityOnHand);
                Assert.Equal("ABC", tool.Supplier);
                Assert.Equal("Note", tool.Notes);
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }

        [Fact]
        public void UpdateToolCommand_UpdatesExistingTool()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var db = new DatabaseService(dbPath);
                IToolService toolService = new ToolService(db);
                var customerService = new CustomerService(db);
                var rentalService = new RentalService(db);
                var vm = new ToolManagementViewModel(toolService, customerService, rentalService);
                var tool = new Tool { ToolNumber = "T1", NameDescription = "Hammer" };
                toolService.AddTool(tool);
                vm.LoadTools();
                vm.SelectedTool = vm.Tools.First();
                vm.SelectedTool.NameDescription = "Updated Hammer";
                vm.UpdateToolCommand.Execute(null);
                var updated = toolService.GetAllTools().First();
                Assert.Equal("Updated Hammer", updated.NameDescription);
                Assert.Equal("Updated Hammer", vm.Tools.First().NameDescription);
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }

        [Fact]
        public void DeleteToolCommand_RemovesTool()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var db = new DatabaseService(dbPath);
                IToolService toolService = new ToolService(db);
                var customerService = new CustomerService(db);
                var rentalService = new RentalService(db);
                var vm = new ToolManagementViewModel(toolService, customerService, rentalService);
                var tool = new Tool { ToolNumber = "T1", NameDescription = "Hammer" };
                toolService.AddTool(tool);
                vm.LoadTools();
                vm.SelectedTool = vm.Tools.First();
                vm.DeleteToolCommand.Execute(null);
                Assert.Empty(toolService.GetAllTools());
                Assert.Empty(vm.Tools);
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }

        [Fact]
        public void OpenRentalsCommand_CanExecuteDependsOnSelectedTool()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var db = new DatabaseService(dbPath);
                IToolService toolService = new ToolService(db);
                var customerService = new CustomerService(db);
                var rentalService = new RentalService(db);
                var vm = new ToolManagementViewModel(toolService, customerService, rentalService);

                Assert.False(vm.OpenRentalsCommand.CanExecute(null));

                toolService.AddTool(new Tool { ToolNumber = "T1", NameDescription = "Hammer" });
                vm.LoadTools();
                vm.SelectedTool = vm.Tools.First();

                Assert.True(vm.OpenRentalsCommand.CanExecute(null));
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }
    }
}
