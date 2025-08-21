// Views/ChangePasswordWindow.xaml.cs
using System;
using System.Windows;
using System.Windows.Controls;
using InventoryManagementApp.ViewModels;
using InventoryManagementApp.Utilities.Extensions;

namespace InventoryManagementApp.Views.Windows
{
    public partial class ChangePasswordWindow : Window, IDisposable
    {
        bool _disposed;

        public ChangePasswordViewModel VM => (ChangePasswordViewModel)DataContext;
        public string NewPassword => VM.NewPassword;

        public ChangePasswordWindow()
        {
            InitializeComponent();
            DataContext = new ChangePasswordViewModel(() => DialogResult = true, () => DialogResult = false);
            this.DisposeDataContextOnUnload();
        }

        void NewPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
            => VM.NewPassword = ((PasswordBox)sender).Password;

        void ConfirmPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
            => VM.ConfirmPassword = ((PasswordBox)sender).Password;

        public void Dispose()
        {
            if (_disposed)
                return;
            _disposed = true;
            Close();
        }
    }
}
