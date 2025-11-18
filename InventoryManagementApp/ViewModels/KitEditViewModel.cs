using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InventoryManagementApp.Models.Domain;

namespace InventoryManagementApp.ViewModels
{
    public class KitEditViewModel : ObservableObject
    {
        public Kit Kit { get; }

        public bool IsNew { get; }

        public string Title => IsNew ? "Create Kit" : "Edit Kit";

        public IRelayCommand SaveCommand { get; }

        public IRelayCommand CancelCommand { get; }

        public KitEditViewModel(Kit kit, bool isNew, Action onSave, Action onCancel)
        {
            Kit = kit;
            IsNew = isNew;
            SaveCommand = new RelayCommand(onSave);
            CancelCommand = new RelayCommand(onCancel);
        }
    }
}
