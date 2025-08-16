using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Documents;
using ToolManagementAppV2.Interfaces;
using ToolManagementAppV2.Models.Domain;
using ToolManagementAppV2.ViewModels;

namespace ToolManagementAppV2.Tests;

public class RecordingDialogService : IDialogService
{
    public List<string> Messages { get; } = new();
    public void ShowInfo(string message, string title) => Messages.Add(message);
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
