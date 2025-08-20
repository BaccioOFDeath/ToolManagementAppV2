using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using ToolManagementAppV2.Interfaces;
using ToolManagementAppV2.ViewModels;
using ToolManagementAppV2.ViewModels.Rental;
using ToolManagementAppV2.Views.Pages;
using ToolManagementAppV2.Views.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace ToolManagementAppV2.Services
{
    public class DialogService : IDialogService
    {
        readonly IServiceProvider _serviceProvider;
        readonly ILogger<DialogService> _logger;

        public DialogService(IServiceProvider serviceProvider, ILogger<DialogService>? logger = null)
        {
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

        public ItemModel? ShowEditToolDialog(ItemModel tool)
        {
            ToolEditWindow? win = null;
            win = ActivatorUtilities.CreateInstance<ToolEditWindow>(_serviceProvider,
                tool,
                (Action)(() => win!.DialogResult = true),
                (Action)(() => win!.DialogResult = false));
            try { win.Owner = System.Windows.Application.Current?.MainWindow; }
            catch (Exception ex) { _logger.LogError(ex, "Failed to set owner for ToolEditWindow"); }
            try { return win.ShowDialog() == true ? tool : null; }
            catch (Exception ex) { _logger.LogError(ex, "Failed to show ToolEditWindow"); return null; }
        }

        public Task<ItemModel?> ShowEditToolDialogAsync(ItemModel tool)
        {
            var dispatcher = System.Windows.Application.Current?.Dispatcher;
            if (dispatcher != null)
                return dispatcher.InvokeAsync(() => ShowEditToolDialog(tool)).Task;

            _logger.LogWarning("No dispatcher available for ShowEditToolDialogAsync; returning null.");
            return Task.FromResult<ItemModel?>(null);
        }

        public void ShowToolDetails(ItemModel tool)
        {
            ToolDetailsWindow win = null!;
            win = new ToolDetailsWindow(tool);
            try { win.Owner = System.Windows.Application.Current?.MainWindow; }
            catch (Exception ex) { _logger.LogError(ex, "Failed to set owner for ToolDetailsWindow"); }
            try { win.ShowDialog(); }
            catch (Exception ex) { _logger.LogError(ex, "Failed to show ToolDetailsWindow"); }
        }

        public (CustomerModel customer, DateTime dueDate)? ShowRentToolDialog(ItemModel tool, IEnumerable<CustomerModel> customers)
        {
            var vm = new RentToolPopupViewModel(tool, customers);
            var win = new RentToolPopupWindow { DataContext = vm };

            EventHandler handler = null!;
            handler = (_, _) => win.Close();
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
            CustomerEditWindow win = null!;
            win = new CustomerEditWindow(customer,
                onSave: () => win.DialogResult = true,
                onCancel: () => win.DialogResult = false);
            try { win.Owner = System.Windows.Application.Current?.MainWindow; }
            catch (Exception ex) { _logger.LogError(ex, "Failed to set owner for CustomerEditWindow"); }
            try { return win.ShowDialog() == true ? customer : null; }
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

        public void ShowRentalHistory(ItemModel tool, IEnumerable<RentalModel> history)
        {
            var vm = new RentalHistoryViewModel(tool, history, this);
            var win = new RentalHistoryWindow(vm) { Title = $"Rental History - {tool.ItemNumber}" };
            try { win.Owner = System.Windows.Application.Current?.MainWindow; }
            catch (Exception ex) { _logger.LogError(ex, "Failed to set owner for RentalHistoryWindow"); }
            try { win.ShowDialog(); }
            catch (Exception ex) { _logger.LogError(ex, "Failed to show RentalHistoryWindow"); }
        }

        public Dictionary<string, string>? ShowImportMapping(IEnumerable<string> headers, IEnumerable<string> propertyNames)
        {
            var win = new ImportMappingWindow(headers, propertyNames);
            try { win.Owner = System.Windows.Application.Current?.MainWindow; }
            catch (Exception ex) { _logger.LogError(ex, "Failed to set owner for ImportMappingWindow"); }
            try
            {
                if (win.ShowDialog() == true)
                {
                    return win.VM.Mappings.ToDictionary(m => m.SelectedColumn!, m => m.PropertyName);
                }
            }
            catch (Exception ex) { _logger.LogError(ex, "Failed to show ImportMappingWindow"); }
            return null;
        }

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

    }
}
