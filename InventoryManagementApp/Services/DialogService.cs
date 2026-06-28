using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using InventoryManagementApp.Interfaces;
using InventoryManagementApp.Models.Domain;
using InventoryManagementApp.Services.Settings;
using InventoryManagementApp.ViewModels;
using InventoryManagementApp.ViewModels.Rental;
using InventoryManagementApp.Views.Pages;
using InventoryManagementApp.Views.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace InventoryManagementApp.Services
{
    public class DialogService : IDialogService
    {
        readonly IServiceProvider _serviceProvider;
        readonly ILogger<DialogService> _logger;

        public DialogService(IServiceProvider serviceProvider, ILogger<DialogService>? logger = null)
        {
            ArgumentNullException.ThrowIfNull(serviceProvider);
            _serviceProvider = serviceProvider;
            _logger = logger ?? NullLogger<DialogService>.Instance;
        }
        public void ShowInfo(string message, string title)
        {
            var dialog = new InfoDialogWindow(message) { Title = title };
            dialog.ShowDialog();
        }

        public Task ShowInfoAsync(string message, string title)
        {
            var dispatcher = System.Windows.Application.Current?.Dispatcher;
            if (dispatcher != null)
                return dispatcher.InvokeAsync(() => ShowInfo(message, title)).Task;

            _logger.LogWarning("No dispatcher available for ShowInfoAsync; dialog not shown.");
            return Task.CompletedTask;
        }

        public bool ShowConfirmation(string message, string title)
        {
            var dialog = new ConfirmDialogWindow(message) { Title = title };
            return dialog.ShowDialog() == true;
        }

        public Task<bool> ShowConfirmationAsync(string message, string title)
        {
            var dispatcher = System.Windows.Application.Current?.Dispatcher;
            if (dispatcher != null)
                return dispatcher.InvokeAsync(() => ShowConfirmation(message, title)).Task;

            _logger.LogWarning("No dispatcher available for ShowConfirmationAsync; returning false.");
            return Task.FromResult(false);
        }

        public ItemModel? ShowEditItemDialog(ItemModel item)
        {
            ArgumentNullException.ThrowIfNull(item);
            ItemEditWindow? win = null;
            win = ActivatorUtilities.CreateInstance<ItemEditWindow>(_serviceProvider,
                item,
                (Action)(() => { if (win != null) win.DialogResult = true; }),
                (Action)(() => { if (win != null) win.DialogResult = false; }));
            try { win.Owner = System.Windows.Application.Current?.MainWindow; }
            catch (Exception ex) { _logger.LogError(ex, "Failed to set owner for ItemEditWindow"); }
            try { return win.ShowDialog() == true ? item : null; }
            catch (Exception ex) { _logger.LogError(ex, "Failed to show ItemEditWindow"); return null; }
        }

        public Task<ItemModel?> ShowEditItemDialogAsync(ItemModel item)
        {
            ArgumentNullException.ThrowIfNull(item);
            var dispatcher = System.Windows.Application.Current?.Dispatcher;
            if (dispatcher != null)
                return dispatcher.InvokeAsync(() => ShowEditItemDialog(item)).Task;

            _logger.LogWarning("No dispatcher available for ShowEditItemDialogAsync; returning null.");
            return Task.FromResult<ItemModel?>(null);
        }

        public void ShowItemDetails(ItemModel item)
        {
            ArgumentNullException.ThrowIfNull(item);
            var win = ActivatorUtilities.CreateInstance<ItemDetailsWindow>(_serviceProvider, item);
            try { win.Owner = System.Windows.Application.Current?.MainWindow; }
            catch (Exception ex) { _logger.LogError(ex, "Failed to set owner for ItemDetailsWindow"); }
            try { win.ShowDialog(); }
            catch (Exception ex) { _logger.LogError(ex, "Failed to show ItemDetailsWindow"); }
        }

        public (CustomerModel customer, DateTime dueDate)? ShowRentItemDialog(ItemModel item, IEnumerable<CustomerModel> customers)
        {
            ArgumentNullException.ThrowIfNull(item);
            ArgumentNullException.ThrowIfNull(customers);
            return InvokeOnDispatcher(() => ShowRentItemDialogCore(item, customers), ((CustomerModel customer, DateTime dueDate)?)null);
        }

        (CustomerModel customer, DateTime dueDate)? ShowRentItemDialogCore(ItemModel item, IEnumerable<CustomerModel> customers)
        {
            var vm = ActivatorUtilities.CreateInstance<RentItemPopupViewModel>(_serviceProvider, item, customers, this);
            var rentalConfigService = _serviceProvider.GetService<RentalConfigurationService>();
            if (rentalConfigService != null)
            {
                try
                {
                    var quickRentalDays = rentalConfigService.GetQuickRentalDaysAsync().GetAwaiter().GetResult();
                    vm.ApplyQuickRentalDays(quickRentalDays);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to load rental quick-day buttons; using defaults.");
                }
            }
            var win = new RentItemPopupWindow { DataContext = vm };

            EventHandler handler = (_, _) => win.Close();
            vm.RequestClose += handler;

            try
            {
                win.ShowDialog();
            }
            finally
            {
                vm.RequestClose -= handler;
            }

            if (vm.SelectedCustomerResult != null)
            {
                return (vm.SelectedCustomerResult, vm.SelectedDueDateResult);
            }

            return null;
        }

        public CustomerModel? ShowAddCustomerDialog()
        {
            var customer = new CustomerModel();
            CustomerEditWindow? win = null;
            win = new CustomerEditWindow(customer,
                onSave: () => { if (win != null) win.DialogResult = true; },
                onCancel: () => { if (win != null) win.DialogResult = false; });
            try { win.Owner = System.Windows.Application.Current?.MainWindow; }
            catch (Exception ex) { _logger.LogError(ex, "Failed to set owner for CustomerEditWindow"); }
            try { return win.ShowDialog() == true ? customer : null; }
            catch (Exception ex) { _logger.LogError(ex, "Failed to show CustomerEditWindow"); return null; }
        }

        public CustomerModel? ShowEditCustomerDialog(CustomerModel customer)
        {
            ArgumentNullException.ThrowIfNull(customer);
            var copy = new CustomerModel
            {
                CustomerID = customer.CustomerID,
                Company = customer.Company,
                Email = customer.Email,
                Contact = customer.Contact,
                Phone = customer.Phone,
                Mobile = customer.Mobile,
                Address = customer.Address
            };
            CustomerEditWindow? win = null;
            win = new CustomerEditWindow(copy,
                onSave: () => { if (win != null) win.DialogResult = true; },
                onCancel: () => { if (win != null) win.DialogResult = false; });
            try { win.Owner = System.Windows.Application.Current?.MainWindow; }
            catch (Exception ex) { _logger.LogError(ex, "Failed to set owner for CustomerEditWindow"); }
            try { return win.ShowDialog() == true ? copy : null; }
            catch (Exception ex) { _logger.LogError(ex, "Failed to show CustomerEditWindow"); return null; }
        }

        public void ShowRentalsFilter(ManageRentalsViewModel viewModel)
        {
            var win = new RentalsFilterWindow { DataContext = viewModel };
            try { win.Owner = System.Windows.Application.Current?.MainWindow; }
            catch (Exception ex) { _logger.LogError(ex, "Failed to set owner for RentalsFilterWindow"); }
            try { win.ShowDialog(); }
            catch (Exception ex) { _logger.LogError(ex, "Failed to show RentalsFilterWindow"); }
        }

        public void ShowRentalHistory(ItemModel item, IEnumerable<RentalModel> history)
        {
            ArgumentNullException.ThrowIfNull(item);
            ArgumentNullException.ThrowIfNull(history);
            var vm = new RentalHistoryViewModel(item, history, this);
            var win = new RentalHistoryWindow(vm) { Title = $"Rental History - {item.ItemNumber}" };
            try { win.Owner = System.Windows.Application.Current?.MainWindow; }
            catch (Exception ex) { _logger.LogError(ex, "Failed to set owner for RentalHistoryWindow"); }
            try { win.ShowDialog(); }
            catch (Exception ex) { _logger.LogError(ex, "Failed to show RentalHistoryWindow"); }
        }

        public Dictionary<string, string>? ShowImportMapping(
            IEnumerable<string> headers,
            IEnumerable<string> propertyNames,
            IEnumerable<string>? requiredPropertyNames = null)
        {
            ArgumentNullException.ThrowIfNull(headers);
            ArgumentNullException.ThrowIfNull(propertyNames);
            var win = CreateImportMappingWindow(headers, propertyNames, requiredPropertyNames);
            try { win.Owner = System.Windows.Application.Current?.MainWindow; }
            catch (Exception ex) { _logger.LogError(ex, "Failed to set owner for ImportMappingWindow"); }
            try
            {
                if (win.ShowDialog() == true)
                {
                    return win.VM.Mappings
                        .Where(m => !string.IsNullOrEmpty(m.SelectedColumn))
                        .ToDictionary(m => m.PropertyName, m => m.SelectedColumn ?? string.Empty);
                }
            }
            catch (Exception ex) { _logger.LogError(ex, "Failed to show ImportMappingWindow"); }
            return null;
        }

        protected virtual ImportMappingWindow CreateImportMappingWindow(
            IEnumerable<string> headers,
            IEnumerable<string> propertyNames,
            IEnumerable<string>? requiredPropertyNames)
            => new ImportMappingWindow(headers, propertyNames, requiredPropertyNames);

        public Func<ItemModel, IEnumerable<string>>? ShowImageImportMapping()
        {
            var win = new ImageImportMappingWindow();
            try { win.Owner = System.Windows.Application.Current?.MainWindow; }
            catch (Exception ex) { _logger.LogError(ex, "Failed to set owner for ImageImportMappingWindow"); }
            try { return win.ShowDialog() == true ? win.VM.BuildSelector() : null; }
            catch (Exception ex) { _logger.LogError(ex, "Failed to show ImageImportMappingWindow"); return null; }
        }

        public void ShowPrintPreview(FlowDocument document, string title, string description)
        {
            ArgumentNullException.ThrowIfNull(document);
            ArgumentNullException.ThrowIfNull(title);
            ArgumentNullException.ThrowIfNull(description);
            InvokeOnDispatcher(() => ShowPrintPreviewCore(document, title, description));
        }

        void ShowPrintPreviewCore(FlowDocument document, string title, string description)
        {
            var win = new PrintPreviewWindow();
            try { win.Owner = System.Windows.Application.Current?.MainWindow; }
            catch (Exception ex) { _logger.LogError(ex, "Failed to set owner for PrintPreviewWindow"); }
            try { win.ShowPreview(document, title, description); }
            catch (Exception ex) { _logger.LogError(ex, "Failed to show PrintPreviewWindow"); }
        }

        public void ShowPrintLabelDialog()
        {
            var win = _serviceProvider.GetRequiredService<PrintLabelWindow>();
            try { win.Owner = System.Windows.Application.Current?.MainWindow; }
            catch (Exception ex) { _logger.LogError(ex, "Failed to set owner for PrintLabelWindow"); }
            try { win.ShowDialog(); }
            catch (Exception ex) { _logger.LogError(ex, "Failed to show PrintLabelWindow"); }
        }


        public Task<string?> ShowInputDialogAsync(string title, string message)
            => InvokeOnDispatcherAsync(() => ShowInputDialog(title, message), (string?)null);

        public Task<bool> ShowMaintenanceEditDialogAsync(MaintenanceRecord record, bool isNew)
        {
            ArgumentNullException.ThrowIfNull(record);
            return InvokeOnDispatcherAsync(() => ShowMaintenanceDialog(record, isNew), false);
        }

        public Task<bool> ShowCalibrationEditDialogAsync(CalibrationRecord record, bool isNew)
        {
            ArgumentNullException.ThrowIfNull(record);
            return InvokeOnDispatcherAsync(() => ShowCalibrationDialog(record, isNew), false);
        }

        public Task<bool> ShowReservationEditDialogAsync(Reservation reservation, bool isNew)
        {
            ArgumentNullException.ThrowIfNull(reservation);
            return InvokeOnDispatcherAsync(() => ShowReservationDialog(reservation, isNew), false);
        }

        public Task<bool> ShowKitEditDialogAsync(Kit kit, bool isNew)
        {
            ArgumentNullException.ThrowIfNull(kit);
            return InvokeOnDispatcherAsync(() => ShowKitDialog(kit, isNew), false);
        }

        public Task<bool> ShowKitItemEditDialogAsync(KitItem kitItem, bool isNew)
        {
            ArgumentNullException.ThrowIfNull(kitItem);
            return InvokeOnDispatcherAsync(() => ShowKitItemDialog(kitItem, isNew), false);
        }

        async Task<T> InvokeOnDispatcherAsync<T>(Func<T> factory, T fallback)
        {
            var dispatcher = System.Windows.Application.Current?.Dispatcher;
            if (dispatcher == null)
            {
                _logger.LogWarning("No dispatcher available for dialog invocation; returning fallback value.");
                return fallback;
            }

            if (dispatcher.CheckAccess())
                return factory();

            return await dispatcher.InvokeAsync(factory);
        }

        void InvokeOnDispatcher(Action action)
        {
            var dispatcher = System.Windows.Application.Current?.Dispatcher;
            if (dispatcher == null || dispatcher.CheckAccess())
            {
                action();
                return;
            }

            dispatcher.Invoke(action);
        }

        T InvokeOnDispatcher<T>(Func<T> factory, T fallback)
        {
            var dispatcher = System.Windows.Application.Current?.Dispatcher;
            if (dispatcher == null)
            {
                _logger.LogWarning("No dispatcher available for dialog invocation; returning fallback value.");
                return fallback;
            }

            if (dispatcher.CheckAccess())
                return factory();

            try
            {
                return dispatcher.Invoke(factory);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to invoke dialog on dispatcher.");
                return fallback;
            }
        }

        string? ShowInputDialog(string title, string message, bool isRequired = false)
        {
            var win = new InputDialogWindow(title, message, isRequired);
            TrySetOwner(win);
            return ShowDialogSafe(win) ? win.ViewModel.InputText : null;
        }

        bool ShowMaintenanceDialog(MaintenanceRecord record, bool isNew)
        {
            var win = new MaintenanceEditWindow(record, isNew);
            TrySetOwner(win);
            return ShowDialogSafe(win);
        }

        bool ShowCalibrationDialog(CalibrationRecord record, bool isNew)
        {
            var win = new CalibrationEditWindow(record, isNew);
            TrySetOwner(win);
            return ShowDialogSafe(win);
        }

        bool ShowReservationDialog(Reservation reservation, bool isNew)
        {
            var win = ActivatorUtilities.CreateInstance<ReservationEditWindow>(_serviceProvider, reservation, isNew);
            TrySetOwner(win);
            return ShowDialogSafe(win);
        }

        bool ShowKitDialog(Kit kit, bool isNew)
        {
            var win = new KitEditWindow(kit, isNew);
            TrySetOwner(win);
            return ShowDialogSafe(win);
        }

        bool ShowKitItemDialog(KitItem kitItem, bool isNew)
        {
            var win = new KitItemEditWindow(kitItem, isNew);
            TrySetOwner(win);
            return ShowDialogSafe(win);
        }

        void TrySetOwner(Window win)
        {
            try { win.Owner = System.Windows.Application.Current?.MainWindow; }
            catch (Exception ex) { _logger.LogError(ex, "Failed to set dialog owner"); }
        }

        bool ShowDialogSafe(Window window)
        {
            try { return window.ShowDialog() == true; }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to show dialog {Dialog}", window.GetType().Name);
                return false;
            }
        }

    }
}
