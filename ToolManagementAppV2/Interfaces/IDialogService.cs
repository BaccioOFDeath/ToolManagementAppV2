using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using ToolManagementAppV2.ViewModels;

namespace ToolManagementAppV2.Interfaces
{
    public interface IDialogService
    {
        void ShowInfo(string message, string title);
        Task ShowInfoAsync(string message, string title) =>
            System.Windows.Application.Current?.Dispatcher?.InvokeAsync(() => ShowInfo(message, title)).Task
            ?? Task.CompletedTask;
        bool ShowConfirmation(string message, string title);
        Task<bool> ShowConfirmationAsync(string message, string title) =>
            System.Windows.Application.Current?.Dispatcher?.InvokeAsync(() => ShowConfirmation(message, title)).Task
            ?? Task.FromResult(false);
        ToolModel? ShowEditToolDialog(ToolModel tool);
        Task<ToolModel?> ShowEditToolDialogAsync(ToolModel tool) =>
            System.Windows.Application.Current?.Dispatcher?.InvokeAsync(() => ShowEditToolDialog(tool)).Task
            ?? Task.FromResult<ToolModel?>(null);
        void ShowToolDetails(ToolModel tool);
        (CustomerModel customer, DateTime dueDate)? ShowRentToolDialog(ToolModel tool, IEnumerable<CustomerModel> customers);
        CustomerModel? ShowAddCustomerDialog();

        void ShowRentalsFilter(ManageRentalsViewModel viewModel);
        void ShowRentalHistory(ToolModel tool, IEnumerable<RentalModel> history);
        Dictionary<string, string>? ShowImportMapping(IEnumerable<string> headers, IEnumerable<string> properties);
        Func<ToolModel, IEnumerable<string>>? ShowImageImportMapping();
        void ShowPrintPreview(FlowDocument document, string title, string description);
        void ShowPrintLabelDialog();
    }
}
