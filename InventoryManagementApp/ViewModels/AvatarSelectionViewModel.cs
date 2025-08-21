using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace InventoryManagementApp.ViewModels
{
    public class AvatarSelectionViewModel : ObservableObject
    {
        public ObservableCollection<Uri> Avatars { get; }

        private string _selectedAvatarPath = string.Empty;
        public string SelectedAvatarPath
        {
            get => _selectedAvatarPath;
            private set => SetProperty(ref _selectedAvatarPath, value);
        }

        public IRelayCommand<Uri> SelectAvatarCommand { get; }

        public AvatarSelectionViewModel(IEnumerable<Uri> avatars, Action onSelected)
        {
            Avatars = new ObservableCollection<Uri>(avatars ?? Array.Empty<Uri>());
            SelectAvatarCommand = new RelayCommand<Uri>(uri =>
            {
                if (uri == null) return;
                SelectedAvatarPath = uri.LocalPath;
                onSelected();
            });
        }
    }
}

