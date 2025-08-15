using System;
using System.IO;
using System.Windows;
using ToolManagementAppV2.Services.Devices;
using ToolManagementAppV2.Services.Core;
using ToolManagementAppV2.Services.Settings;
using ToolManagementAppV2.ViewModels;
using ToolManagementAppV2.Utilities.Extensions;

namespace ToolManagementAppV2.Views
{
    /// <summary>
    /// Interaction logic for ScannerStatusWindow.xaml
    /// </summary>
    public partial class ScannerStatusWindow : Window
    {
        readonly DatabaseService _ownedDb;

        public ScannerStatusWindow()
        {
            InitializeComponent();
            var dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tool_inventory.db");
            _ownedDb = new DatabaseService(dbPath);
            var settingsService = new SettingsService(_ownedDb);
            DataContext = new ScannerStatusViewModel(new ScannerService(settingsService));
            this.DisposeDataContextOnUnload();
            Closed += (_, __) => _ownedDb.Dispose();
        }
    }
}
