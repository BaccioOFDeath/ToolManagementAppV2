// MainWindow.xaml.cs
using System;
using System.Windows;
using ToolManagementAppV2.Services.Core;
using ToolManagementAppV2.ViewModels;
using ToolManagementAppV2.Utilities.Extensions;

namespace ToolManagementAppV2
{
    public partial class MainWindow : Window
    {
        readonly DatabaseService? _ownedDb;

        /// <summary>
        /// Initializes a new instance of <see cref="MainWindow"/> with the provided services.
        /// When <paramref name="ownedDatabaseService"/> is supplied, the window will dispose it
        /// when closed.
        /// </summary>
        /// <param name="viewModel">View model to use as the window's data context.</param>
        /// <param name="ownedDatabaseService">Database service owned by the window; disposed on close.</param>
        public MainWindow(MainViewModel viewModel, DatabaseService? ownedDatabaseService = null)
        {
            InitializeComponent();

            DataContext = viewModel;
            _ownedDb = ownedDatabaseService;
            this.DisposeDataContextOnUnload();

            Closed += (_, __) => _ownedDb?.Dispose();
        }
    }
}
