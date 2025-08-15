// ViewModels/UsersEditViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ToolManagementAppV2.Models.Domain;
using System;
using System.Threading.Tasks;

namespace ToolManagementAppV2.ViewModels
{
    public class UsersEditViewModel : ObservableObject
    {
        public string Title { get; }
        public User EditingUser { get; }

        public IRelayCommand BrowseAvatarCommand { get; }
        public IRelayCommand RemoveAvatarCommand { get; }
        public IAsyncRelayCommand SaveCommand { get; }
        public IRelayCommand CancelCommand { get; }

        public UsersEditViewModel(User user, Func<Task> onSave, Action onCancel, Action onBrowseAvatar, Action onRemoveAvatar)
        {
            EditingUser = user ?? new User();
            Title = (EditingUser.UserID == 0) ? "Add User" : "Edit User";
            BrowseAvatarCommand = new RelayCommand(onBrowseAvatar);
            RemoveAvatarCommand = new RelayCommand(onRemoveAvatar);
            SaveCommand = new AsyncRelayCommand(onSave);
            CancelCommand = new RelayCommand(onCancel);
        }
    }
}
