// MainWindow.xaml.cs
using System;
using System.IO;
using System.Windows;
using ToolManagementAppV2.Services.Core;
using ToolManagementAppV2.Services.Tools;
using ToolManagementAppV2.Services.Customers;
using ToolManagementAppV2.Services.Users;
using ToolManagementAppV2.Services.Rentals;
using ToolManagementAppV2.Services.Settings;
using ToolManagementAppV2.Services;
using ToolManagementAppV2.ViewModels;

namespace ToolManagementAppV2
{
    public partial class MainWindow : Window
    {
        readonly DatabaseService? _ownedDb;

        /// <summary>
        /// Creates a <see cref="MainWindow"/> with its own internally managed services.
        /// The window owns the database service and will dispose it when closed.
        /// </summary>
        public MainWindow() : this(null, null)
        {
        }

        /// <summary>
        /// Creates a <see cref="MainWindow"/> with a supplied <paramref name="viewModel"/>.
        /// When <paramref name="ownedDatabaseService"/> is provided, the window will dispose it
        /// upon closing. If <c>null</c>, the caller is responsible for managing the database
        /// service's lifetime.
        /// </summary>
        /// <param name="viewModel">Optional view model to use as the window's data context.</param>
        /// <param name="ownedDatabaseService">Database service owned by the window; disposed on close.</param>
        public MainWindow(MainViewModel? viewModel, DatabaseService? ownedDatabaseService = null)
        {
            InitializeComponent();

            if (viewModel != null)
            {
                DataContext = viewModel;
                _ownedDb = ownedDatabaseService;
            }
            else
            {
                _ownedDb = new DatabaseService(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tool_inventory.db"));
                var toolService = new ToolService(_ownedDb);
                var customerService = new CustomerService(_ownedDb);
                var userContext = new ApplicationUserContext();
                var userService = new UserService(_ownedDb, userContext);
                var rentalService = new RentalService(_ownedDb, toolService);
                var activityLogService = new ActivityLogService(_ownedDb);
                var fileDialogService = new FileDialogService();
                var settingsService = new SettingsService(_ownedDb);
                var dialogService = new DialogService();

                DataContext = new MainViewModel(toolService, userService, userContext, customerService, rentalService, fileDialogService, activityLogService, settingsService, _ownedDb, dialogService);
            }

            Closed += (_, __) => _ownedDb?.Dispose();
        }
    }
}
