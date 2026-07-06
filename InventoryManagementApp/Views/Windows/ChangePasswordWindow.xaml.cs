// Views/ChangePasswordWindow.xaml.cs
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using InventoryManagementApp.ViewModels;
using InventoryManagementApp.Utilities.Extensions;

namespace InventoryManagementApp.Views.Windows
{
    public partial class ChangePasswordWindow : Window, IDisposable
    {
        bool _disposed;
        bool _isUnloaded;
        DispatcherOperation? _pendingFocusOperation;

        public ChangePasswordViewModel VM => (ChangePasswordViewModel)DataContext;
        public string NewPassword => VM.NewPassword;

        public ChangePasswordWindow()
        {
            InitializeComponent();
            DataContext = new ChangePasswordViewModel(() => DialogResult = true, () => DialogResult = false);
            this.DisposeDataContextOnUnload();
            Loaded += ChangePasswordWindow_Loaded;
            Activated += ChangePasswordWindow_Activated;
            Unloaded += ChangePasswordWindow_Unloaded;
        }

        void ChangePasswordWindow_Loaded(object sender, RoutedEventArgs e)
        {
            _isUnloaded = false;
            FocusNewPasswordBox(selectAll: true);
        }

        void ChangePasswordWindow_Activated(object? sender, EventArgs e)
        {
            FocusNewPasswordBox();
        }

        void ChangePasswordWindow_Unloaded(object sender, RoutedEventArgs e)
        {
            _isUnloaded = true;
            AbortPendingFocus();
        }

        void NewPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
            => VM.NewPassword = ((PasswordBox)sender).Password;

        void ConfirmPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
            => VM.ConfirmPassword = ((PasswordBox)sender).Password;

        void PasswordBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && VM.SaveCommand.CanExecute(null))
            {
                VM.SaveCommand.Execute(null);
                e.Handled = true;
                return;
            }

            if (e.Key == Key.Escape && VM.CancelCommand.CanExecute(null))
            {
                VM.CancelCommand.Execute(null);
                e.Handled = true;
            }
        }

        void FocusNewPasswordBox(bool selectAll = false)
        {
            if (_isUnloaded || !IsLoaded)
                return;

            AbortPendingFocus();
            _pendingFocusOperation = NewPasswordBox.Dispatcher.BeginInvoke(() =>
            {
                _pendingFocusOperation = null;

                if (_isUnloaded || !IsLoaded)
                    return;

                NewPasswordBox.Focus();
                Keyboard.Focus(NewPasswordBox);

                if (selectAll)
                    NewPasswordBox.SelectAll();
            }, DispatcherPriority.Input);
        }

        void AbortPendingFocus()
        {
            if (_pendingFocusOperation is { Status: DispatcherOperationStatus.Pending })
                _pendingFocusOperation.Abort();

            _pendingFocusOperation = null;
        }

        public void Dispose()
        {
            if (_disposed)
                return;
            _disposed = true;
            AbortPendingFocus();
            Close();
        }
    }
}