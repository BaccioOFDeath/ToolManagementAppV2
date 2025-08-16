// MainWindow.xaml.cs
using System;
using System.Windows;
using ToolManagementAppV2.Utilities.Extensions;
using ToolManagementAppV2.Interfaces;

namespace ToolManagementAppV2
{
    public partial class MainWindow : Window, IMainWindow
    {
        readonly IDatabaseService? _ownedDb;

        /// <summary>
        /// Initializes a new instance of <see cref="MainWindow"/> with the provided services.
        /// When <paramref name="ownedDatabaseService"/> is supplied, the window will dispose it
        /// when closed.
        /// </summary>
        /// <param name="viewModel">View model to use as the window's data context.</param>
        /// <param name="ownedDatabaseService">Database service owned by the window; disposed on close.</param>
        public MainWindow(IMainViewModel viewModel, IDatabaseService? ownedDatabaseService = null)
        {
            InitializeComponent();

            DataContext = viewModel;
            _ownedDb = ownedDatabaseService;
            this.DisposeDataContextOnUnload();

            Closed += (_, __) => _ownedDb?.Dispose();
        }

        void IMainWindow.Activate() => base.Activate();

        void IMainWindow.Focus() => base.Focus();
    }
}
