using System.IO;
using System.Windows;
using ToolManagementAppV2.Services.Core;
using ToolManagementAppV2.Services.Settings;


namespace ToolManagementAppV2.Views
{
    public partial class AvatarSelectionWindow : Window
    {
        public string SelectedAvatarPath { get; private set; }
        public Uri[] Avatars { get; private set; } = Array.Empty<Uri>();

        public AvatarSelectionWindow()
        {
            InitializeComponent();

            var dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tool_inventory.db");
            var dbService = new DatabaseService(dbPath);
            var settingsService = new SettingsService(dbService);
            var appName = settingsService.GetSetting("ApplicationName");
            if (!string.IsNullOrWhiteSpace(appName))
                Title = $"{appName} – Select Avatar";

            var avatarDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "Avatars");
            if (Directory.Exists(avatarDir))
                Avatars = Directory
                    .EnumerateFiles(avatarDir, "*.png")
                    .Select(path => new Uri(path, UriKind.Absolute))
                    .ToArray();

            DataContext = this;
        }

        private void AvatarButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement fe && fe.Tag is Uri uri)
            {
                SelectedAvatarPath = uri.LocalPath;
                DialogResult = true;
            }
        }
    }
}
