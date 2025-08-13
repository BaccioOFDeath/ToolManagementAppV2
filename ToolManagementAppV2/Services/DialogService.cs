using System;
using System.Collections.Generic;
using ToolManagementAppV2.Interfaces;
using ToolManagementAppV2.ViewModels;
using ToolManagementAppV2.ViewModels.Rental;
using ToolManagementAppV2.Views;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace ToolManagementAppV2.Services
{
    public class DialogService : IDialogService
    {
        readonly ILogger<DialogService> _logger;

        public DialogService(ILogger<DialogService>? logger = null)
        {
            _logger = logger ?? NullLogger<DialogService>.Instance;
        }
        public void ShowInfo(string message, string title)
        {
            var dialog = new InfoDialogWindow(message) { Title = title };
            dialog.ShowDialog();
        }

        public bool ShowConfirmation(string message, string title)
        {
            var dialog = new ConfirmDialogWindow(message) { Title = title };
            return dialog.ShowDialog() == true;
        }

        public ToolModel? ShowEditToolDialog(ToolModel tool)
        {
            ToolEditWindow win = null!;
            win = new ToolEditWindow(tool,
                onSave: () => win.DialogResult = true,
                onCancel: () => win.DialogResult = false);
            try { win.Owner = System.Windows.Application.Current?.MainWindow; }
            catch (Exception ex) { _logger.LogError(ex, "Failed to set owner for ToolEditWindow"); }
            try { return win.ShowDialog() == true ? tool : null; }
            catch (Exception ex) { _logger.LogError(ex, "Failed to show ToolEditWindow"); return null; }
        }

        public void ShowToolDetails(ToolModel tool)
        {
            ToolDetailsWindow win = null!;
            win = new ToolDetailsWindow(tool);
            try { win.Owner = System.Windows.Application.Current?.MainWindow; }
            catch (Exception ex) { _logger.LogError(ex, "Failed to set owner for ToolDetailsWindow"); }
            try { win.ShowDialog(); }
            catch (Exception ex) { _logger.LogError(ex, "Failed to show ToolDetailsWindow"); }
        }

        public (CustomerModel customer, DateTime dueDate)? ShowRentToolDialog(ToolModel tool, IEnumerable<CustomerModel> customers)
        {
            var vm = new RentToolPopupViewModel(tool, customers);
            var win = new RentToolPopupWindow { DataContext = vm };
            vm.RequestClose += (_, _) => win.Close();
            win.ShowDialog();

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

        public void ShowRentalHistory(ToolModel tool, IEnumerable<RentalModel> history)
        {
            var vm = new RentalHistoryViewModel(tool, history);
            var win = new RentalHistoryWindow(vm) { Title = $"Rental History - {tool.ToolNumber}" };
            try { win.Owner = System.Windows.Application.Current?.MainWindow; }
            catch (Exception ex) { _logger.LogError(ex, "Failed to set owner for RentalHistoryWindow"); }
            try { win.ShowDialog(); }
            catch (Exception ex) { _logger.LogError(ex, "Failed to show RentalHistoryWindow"); }
        }
    }
}
