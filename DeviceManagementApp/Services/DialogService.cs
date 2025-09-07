using System;
using System.Windows;
using DeviceManagementApp.Interfaces;
using DeviceManagementApp.Views.Windows;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace DeviceManagementApp.Services
{
    public class DialogService : IDialogService
    {
        private readonly Func<string, InfoDialogWindow> _infoFactory;
        private readonly Func<string, ConfirmDialogWindow> _confirmFactory;
        private readonly ILogger<DialogService> _logger;

        public DialogService(
            Func<string, InfoDialogWindow> infoFactory,
            Func<string, ConfirmDialogWindow> confirmFactory,
            ILogger<DialogService>? logger = null)
        {
            _infoFactory = infoFactory;
            _confirmFactory = confirmFactory;
            _logger = logger ?? NullLogger<DialogService>.Instance;
        }

        public void ShowInfo(string message, string title)
        {
            var dialog = _infoFactory(message);
            dialog.Title = title;
            try { dialog.Owner = Application.Current?.MainWindow; }
            catch (Exception ex) { _logger.LogError(ex, "Failed to set owner for InfoDialogWindow"); }
            dialog.ShowDialog();
        }

        public bool ShowConfirmation(string message, string title)
        {
            var dialog = _confirmFactory(message);
            dialog.Title = title;
            try { dialog.Owner = Application.Current?.MainWindow; }
            catch (Exception ex) { _logger.LogError(ex, "Failed to set owner for ConfirmDialogWindow"); }
            return dialog.ShowDialog() == true;
        }
    }
}
