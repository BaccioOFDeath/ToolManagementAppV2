using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using InventoryManagementApp.ViewModels;

namespace InventoryManagementApp.Interfaces
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
        ItemModel? ShowEditItemDialog(ItemModel item);
        Task<ItemModel?> ShowEditItemDialogAsync(ItemModel item) =>
            System.Windows.Application.Current?.Dispatcher?.InvokeAsync(() => ShowEditItemDialog(item)).Task
            ?? Task.FromResult<ItemModel?>(null);
        void ShowItemDetails(ItemModel item);
        (CustomerModel customer, DateTime dueDate)? ShowRentItemDialog(ItemModel item, IEnumerable<CustomerModel> customers);
        CustomerModel? ShowAddCustomerDialog();

        void ShowRentalsFilter(ManageRentalsViewModel viewModel);
        void ShowRentalHistory(ItemModel item, IEnumerable<RentalModel> history);
        Dictionary<string, string>? ShowImportMapping(IEnumerable<string> headers, IEnumerable<string> properties);
        Func<ItemModel, IEnumerable<string>>? ShowImageImportMapping();
        void ShowPrintPreview(FlowDocument document, string title, string description);
        void ShowPrintLabelDialog();
    }
}
