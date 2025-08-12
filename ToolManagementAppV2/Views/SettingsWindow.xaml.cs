using System;
using System.Windows;
using System.Collections.Generic;
using System.IO;
using ToolManagementAppV2.Services.Core;
using ToolManagementAppV2.Services.Settings;
using ToolManagementAppV2.ViewModels;

namespace ToolManagementAppV2.Views
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();

            var db = new DatabaseService(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tool_inventory.db"));
            var settingsService = new SettingsService(db);
            var fileDialog = new FileDialogService();
            var settingsVm = new SettingsViewModel(fileDialog, settingsService);

            DataContext = new SettingsWindowViewModel(
                settingsVm,
                () => Close(),
                () =>
                {
                    var dict = new Dictionary<string, string>
                    {
                        ["ApplicationName"] = settingsVm.ApplicationName ?? string.Empty,
                        ["CompanyLogoPath"] = settingsVm.CompanyLogoPath ?? string.Empty,
                        ["DefaultRentalDuration"] = settingsVm.DefaultRentalDuration.ToString(),
                        ["ConnectionString"] = settingsVm.ConnectionString ?? string.Empty,
                        ["Theme"] = settingsVm.Theme ?? string.Empty
                    };
                    settingsService.UpdateSettings(dict);
                });
        }
    }
}
