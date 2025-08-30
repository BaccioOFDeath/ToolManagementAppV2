using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using InventoryManagementApp.Interfaces;
using InventoryManagementApp.Utilities.Extensions;
using InventoryManagementApp.ViewModels;

namespace InventoryManagementApp.Views.Windows
{
    public partial class SetupWizardWindow : Window, ISetupWizard, IDisposable
    {
        bool _disposed;
        readonly SetupWizardViewModel _viewModel;

        public SetupWizardWindow()
        {
            InitializeComponent();
            _viewModel = new SetupWizardViewModel(() => DialogResult = true, () => DialogResult = false, pwd =>
            {
                NewPasswordBox.Password = pwd;
                ConfirmPasswordBox.Password = pwd;
            });
            DataContext = _viewModel;
            this.DisposeDataContextOnUnload();
        }

        void NewPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
            => _viewModel.NewPassword = ((PasswordBox)sender).Password;

        void ConfirmPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
            => _viewModel.ConfirmPassword = ((PasswordBox)sender).Password;

        public Task<SetupWizardResult?> RunAsync()
        {
            var result = ShowDialog() == true
                ? new SetupWizardResult(
                    _viewModel.NewPassword,
                    _viewModel.IsRandom,
                    _viewModel.ApplicationName,
                    _viewModel.ItemLabelSingular,
                    _viewModel.ItemLabelPlural)
                : null;
            return Task.FromResult(result);
        }

        public void Dispose()
        {
            if (_disposed)
                return;
            _disposed = true;
            Close();
        }
    }
}
