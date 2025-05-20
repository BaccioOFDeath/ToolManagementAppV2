using System;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;

namespace ToolManagementAppV2.Views
{
    public partial class AvatarSelectionWindow : Window
    {
        public string SelectedAvatarPath { get; private set; }

        public AvatarSelectionWindow()
        {
            InitializeComponent();
            LoadAvatars();
        }

        private void LoadAvatars()
        {
            var exeDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var avatarDir = Path.Combine(exeDir, "Resources", "Avatars");
            if (!Directory.Exists(avatarDir)) return;

            foreach (var file in Directory.GetFiles(avatarDir, "*.png"))
                AvatarListBox.Items.Add(file);
        }

        private void AvatarButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is string path)
            {
                SelectedAvatarPath = path;
                DialogResult = true;
                Close();
            }
        }
    }
}
