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
            InvokeAsync(() => ShowInfo(message, title));
        bool ShowConfirmation(string message, string title);
        Task<bool> ShowConfirmationAsync(string message, string title) =>
            InvokeAsync(() => ShowConfirmation(message, title));
        ItemModel? ShowEditItemDialog(ItemModel item);
        Task<ItemModel?> ShowEditItemDialogAsync(ItemModel item) =>
            InvokeAsync(() => ShowEditItemDialog(item));
        void ShowItemDetails(ItemModel item);
        (CustomerModel customer, DateTime dueDate)? ShowRentItemDialog(ItemModel item, IEnumerable<CustomerModel> customers);
        CustomerModel? ShowAddCustomerDialog();
        CustomerModel? ShowEditCustomerDialog(CustomerModel customer);
        Task<CustomerModel?> ShowEditCustomerDialogAsync(CustomerModel customer) =>
            InvokeAsync(() => ShowEditCustomerDialog(customer));

        void ShowRentalsFilter(ManageRentalsViewModel viewModel);
        void ShowRentalHistory(ItemModel item, IEnumerable<RentalModel> history);
        Dictionary<string, string>? ShowImportMapping(
            IEnumerable<string> headers,
            IEnumerable<string> properties,
            IEnumerable<string>? requiredPropertyNames = null);
        Func<ItemModel, IEnumerable<string>>? ShowImageImportMapping();
        void ShowPrintPreview(FlowDocument document, string title, string description);
        void ShowPrintLabelDialog();

        Task<bool> ShowConfirmAsync(string title, string message) =>
            ShowConfirmationAsync(message, title);

        Task ShowErrorAsync(string title, string message) =>
            ShowInfoAsync(message, title);

        Task<string?> ShowInputDialogAsync(string title, string message) =>
            Task.FromResult<string?>(null);

        Task<bool> ShowMaintenanceEditDialogAsync(InventoryManagementApp.Models.Domain.MaintenanceRecord record, bool isNew) =>
            Task.FromResult(false);

        Task<bool> ShowCalibrationEditDialogAsync(InventoryManagementApp.Models.Domain.CalibrationRecord record, bool isNew) =>
            Task.FromResult(false);

        Task<bool> ShowReservationEditDialogAsync(InventoryManagementApp.Models.Domain.Reservation reservation, bool isNew) =>
            Task.FromResult(false);

        Task<bool> ShowKitEditDialogAsync(InventoryManagementApp.Models.Domain.Kit kit, bool isNew) =>
            Task.FromResult(false);

        Task<bool> ShowKitItemEditDialogAsync(InventoryManagementApp.Models.Domain.KitItem kitItem, bool isNew) =>
            Task.FromResult(false);

        private static Task InvokeAsync(Action action)
        {
            var dispatcher = System.Windows.Application.Current?.Dispatcher;
            if (dispatcher == null || dispatcher.CheckAccess())
            {
                action();
                return Task.CompletedTask;
            }

            return dispatcher.InvokeAsync(action).Task;
        }

        private static Task<T> InvokeAsync<T>(Func<T> func)
        {
            var dispatcher = System.Windows.Application.Current?.Dispatcher;
            if (dispatcher == null || dispatcher.CheckAccess())
                return Task.FromResult(func());

            return dispatcher.InvokeAsync(func).Task;
        }
    }
}
