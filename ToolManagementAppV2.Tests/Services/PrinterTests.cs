using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Documents;
using ToolManagementAppV2.Interfaces;
using ToolManagementAppV2.Services.Tools;
using ToolManagementAppV2.Models.Domain;
using ToolManagementAppV2.ViewModels;
using CustomerModel = ToolManagementAppV2.Models.Domain.Customer;
using RentalModel = ToolManagementAppV2.Models.Domain.Rental;
using Xunit;
using ToolModel = ToolManagementAppV2.Models.Domain.Tool;

namespace ToolManagementAppV2.Tests.Services
{
    public class PrinterTests
    {
        private class DummySettingsService : ISettingsService
        {
            public void SaveSetting(string key, string value) { }
            public string? GetSetting(string key) => null;
            public Dictionary<string, string> GetAllSettings() => new();
            public void UpdateSettings(Dictionary<string, string> settings) { }
            public void DeleteSetting(string key) { }
            public IEnumerable<string> GetScannerIpAddresses() => Enumerable.Empty<string>();
            public IEnumerable<string> SaveScannerIpAddresses(IEnumerable<string>? ipAddresses) => Enumerable.Empty<string>();
            public int GetPasswordIterations() => 0;
            public void SavePasswordIterations(int iterations) { }
        }

        private class InvalidLogoSettingsService : ISettingsService
        {
            public void SaveSetting(string key, string value) { }
            public string? GetSetting(string key) => ".." + System.IO.Path.DirectorySeparatorChar + "logo.png";
            public Dictionary<string, string> GetAllSettings() => new();
            public void UpdateSettings(Dictionary<string, string> settings) { }
            public void DeleteSetting(string key) { }
            public IEnumerable<string> GetScannerIpAddresses() => Enumerable.Empty<string>();
            public IEnumerable<string> SaveScannerIpAddresses(IEnumerable<string>? ipAddresses) => Enumerable.Empty<string>();
            public int GetPasswordIterations() => 0;
            public void SavePasswordIterations(int iterations) { }
        }

        private class StubDialogService : IDialogService
        {
            public string? LastInfoMessage { get; private set; }
            public void ShowInfo(string message, string title) => LastInfoMessage = message;
            public bool ShowConfirmation(string message, string title) => false;
            public ToolModel? ShowEditToolDialog(ToolModel tool) => null;
            public void ShowToolDetails(ToolModel tool) { }
            public (CustomerModel customer, DateTime dueDate)? ShowRentToolDialog(ToolModel tool, IEnumerable<CustomerModel> customers) => null;
            public CustomerModel? ShowAddCustomerDialog() => null;
            public void ShowRentalsFilter(ManageRentalsViewModel viewModel) { }
            public void ShowRentalHistory(ToolModel tool, IEnumerable<RentalModel> history) { }
            public Dictionary<string, string>? ShowImportMapping(IEnumerable<string> headers, IEnumerable<string> properties) => null;
            public Func<ToolModel, IEnumerable<string>>? ShowImageImportMapping() => null;
            public void ShowPrintPreview(FlowDocument document, string title, string description) { }
            public void ShowPrintLabelDialog() { }
            public void ShowScannerStatus() { }
        }

        [Fact]
        public async Task BuildDocumentIncrementallyAsync_ProcessesAllTools()
        {
            var printer = new Printer(new DummySettingsService());
            var tools = Enumerable.Range(0, 120).Select(i => new ToolModel
            {
                ToolNumber = $"T{i}",
                Location = i.ToString()
            });

            var method = typeof(Printer).GetMethod("BuildDocumentIncrementallyAsync", BindingFlags.NonPublic | BindingFlags.Instance);
            var task = (Task<FlowDocument>)method.Invoke(printer, new object[] { tools, "Title", null, null, 25 });
            var doc = await task;

            Assert.Equal(1 + 120, doc.Blocks.Count);
        }

        [Fact]
        public void LoadCompanyLogoPath_InvalidPath_NotifiesUser()
        {
            var dialog = new StubDialogService();
            var printer = new Printer(new InvalidLogoSettingsService(), dialog);
            var method = typeof(Printer).GetMethod("LoadCompanyLogoPath", BindingFlags.NonPublic | BindingFlags.Instance);
            var result = (string?)method.Invoke(printer, null);
            Assert.Null(result);
            Assert.Equal("Company logo path is invalid.", dialog.LastInfoMessage);
        }
    }
}
