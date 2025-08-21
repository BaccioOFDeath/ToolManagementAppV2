using System;
using System.Windows;
using InventoryManagementApp.Interfaces;
using InventoryManagementApp.Utilities.Extensions;

namespace InventoryManagementApp.Views.Windows
{
    /// <summary>
    /// Interaction logic for LoginWindow.xaml.
    /// Provides an <see cref="IUserContext"/> and optional <see cref="IDialogService"/>
    /// to the <see cref="LoginViewModel"/>.
    /// </summary>
    public partial class LoginWindow : Window, ILoginWindow
    {
        public ILoginViewModel ViewModel => (ILoginViewModel)DataContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="LoginWindow"/> class.
        /// </summary>
        public LoginWindow(ILoginViewModel viewModel)
        {
            InitializeComponent();

            DataContext = viewModel;
            this.DisposeDataContextOnUnload();

            viewModel.LoginSucceeded += OnLoginSucceeded;
            Closed += (_, __) => viewModel.LoginSucceeded -= OnLoginSucceeded;
        }

        private void OnLoginSucceeded(object? sender, EventArgs e)
        {
            DialogResult = true;
            Close();
        }
    }
}
