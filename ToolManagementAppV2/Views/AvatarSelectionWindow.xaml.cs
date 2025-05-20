using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using ToolManagementAppV2.Services;

namespace ToolManagementAppV2.Views
{
    public partial class AvatarSelectionWindow : Window
    {
        public string SelectedAvatarPath { get; private set; }
        public Uri[] Avatars { get; private set; }

        public AvatarSelectionWindow()
        {
            InitializeComponent();

            // 1) Load the application name from settings into the Title
            var db = new DatabaseService("tool_inventory.db");
            var setts = new SettingsService(db);
            var app = setts.GetSetting("ApplicationName");
            if (!string.IsNullOrWhiteSpace(app))
                this.Title = $"{app} – Select Avatar";

            // 2) Load all avatars from Resources\Avatars
            var exeDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var avatarDir = Path.Combine(exeDir, "Resources", "Avatars");
            Avatars = Directory.Exists(avatarDir)
                      ? Directory.GetFiles(avatarDir, "*.png")
                                 .Select(f => new Uri(f, UriKind.Absolute))
                                 .ToArray()
                      : Array.Empty<Uri>();

            DataContext = this;
        }

        private void AvatarButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement fe && fe.Tag is Uri uri)
            {
                SelectedAvatarPath = uri.LocalPath;
                DialogResult = true;
                Close();
            }
        }
    }
}
