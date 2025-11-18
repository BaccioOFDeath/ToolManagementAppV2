using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InventoryManagementApp.Models.Domain;

namespace InventoryManagementApp.ViewModels
{
    public class KitItemEditViewModel : ObservableObject
    {
        public KitItem KitItem { get; }

        public bool IsNew { get; }

        public string Title => IsNew ? "Add Kit Item" : "Edit Kit Item";

        public IRelayCommand SaveCommand { get; }

        public IRelayCommand CancelCommand { get; }

        public KitItemEditViewModel(KitItem kitItem, bool isNew, Action onSave, Action onCancel)
        {
            KitItem = kitItem;
            IsNew = isNew;
            SaveCommand = new RelayCommand(onSave);
            CancelCommand = new RelayCommand(onCancel);
        }
    }
}
