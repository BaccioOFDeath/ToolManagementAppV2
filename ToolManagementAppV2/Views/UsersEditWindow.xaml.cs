// Views/UsersEditWindow.xaml.cs
using System;
using System.Threading.Tasks;
using System.Windows;
using ToolManagementAppV2.Models.Domain;
using ToolManagementAppV2.ViewModels;
using ToolManagementAppV2.Utilities.Extensions;

namespace ToolManagementAppV2.Views
{
    public partial class UsersEditWindow : Window
    {
        public UsersEditWindow(User user, Func<Task> onSave, Action onCancel, Action onRemoveAvatar)
        {
            InitializeComponent();
            DataContext = new UsersEditViewModel(user, onSave, onCancel, BrowseAvatar, onRemoveAvatar);
            this.DisposeDataContextOnUnload();
        }

        private void BrowseAvatar()
        {
            var vm = (UsersEditViewModel)DataContext;
            var avatarWin = new AvatarSelectionWindow();

            try { avatarWin.Owner = this; } catch { }

            try
            {
                if (avatarWin.ShowDialog() == true && !string.IsNullOrEmpty(avatarWin.SelectedAvatarPath))
                {
                    vm.EditingUser.UserPhotoPath = avatarWin.SelectedAvatarPath;
                }
            }
            catch
            {
                // Ignore UI errors in non-interactive environments
            }
        }
    }
}
