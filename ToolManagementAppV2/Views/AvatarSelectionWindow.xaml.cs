using System;
using System.IO;
using System.Linq;
using System.Windows;
using ToolManagementAppV2.Services.Core;
using ToolManagementAppV2.Services.Settings;
using ToolManagementAppV2.Interfaces;
using ToolManagementAppV2.ViewModels;
using ToolManagementAppV2.Utilities.Extensions;


namespace ToolManagementAppV2.Views
{
    public partial class AvatarSelectionWindow : Window
    {
        public AvatarSelectionViewModel VM => (AvatarSelectionViewModel)DataContext;
        public string SelectedAvatarPath => VM.SelectedAvatarPath;

        public AvatarSelectionWindow()
        {
            InitializeComponent();

            var dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tool_inventory.db");
            using var dbService = new DatabaseService(dbPath);
            var settingsService = new SettingsService(dbService);
            var appName = settingsService.GetSetting("ApplicationName");
            if (!string.IsNullOrWhiteSpace(appName))
                Title = $"{appName} – Select Avatar";

            var avatarDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "Avatars");
            var avatars = Array.Empty<Uri>();
            if (Directory.Exists(avatarDir))
                avatars = Directory
                    .EnumerateFiles(avatarDir, "*.png")
                    .Select(path => new Uri(path, UriKind.Absolute))
                    .ToArray();

            DataContext = new AvatarSelectionViewModel(avatars, () => DialogResult = true);
            this.DisposeDataContextOnUnload();
        }
    }
}
