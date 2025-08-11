using System;
using System.IO;
using System.Windows;
using ToolManagementAppV2.Services.Core;
using ToolManagementAppV2.Services.Tools;
using ToolManagementAppV2.Services.Customers;
using ToolManagementAppV2.Services.Users;
using ToolManagementAppV2.Services.Rentals;
using ToolManagementAppV2.ViewModels;

namespace ToolManagementAppV2
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            var db = new DatabaseService(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tool_inventory.db"));
            var toolService = new ToolService(db);
            var customerService = new CustomerService(db);
            var userService = new UserService(db);
            var rentalService = new RentalService(db);
            var fileDialogService = new FileDialogService();
            DataContext = new MainViewModel(toolService, userService, customerService, rentalService, fileDialogService);
        }
    }
}
