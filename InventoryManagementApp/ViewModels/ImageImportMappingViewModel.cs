using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using InventoryManagementApp.Models.Domain;

namespace InventoryManagementApp.ViewModels
{
    public class ImageImportMappingViewModel : ObservableObject
    {
        bool _useItemNumber = true;
        public bool UseItemNumber { get => _useItemNumber; set => SetProperty(ref _useItemNumber, value); }

        bool _usePartNumber;
        public bool UsePartNumber { get => _usePartNumber; set => SetProperty(ref _usePartNumber, value); }

        bool _useName;
        public bool UseName { get => _useName; set => SetProperty(ref _useName, value); }

        public IRelayCommand OkCommand { get; }
        public IRelayCommand CancelCommand { get; }

        public ImageImportMappingViewModel(Action onOk, Action onCancel)
        {
            OkCommand = new RelayCommand(onOk);
            CancelCommand = new RelayCommand(onCancel);
        }

        public Func<ItemModel, IEnumerable<string>> BuildSelector()
        {
            return t =>
            {
                var keys = new List<string>();
                if (UseItemNumber && !string.IsNullOrWhiteSpace(t.ItemNumber))
                    keys.Add(t.ItemNumber.Trim().ToUpperInvariant());
                if (UsePartNumber && !string.IsNullOrWhiteSpace(t.PartNumber))
                    keys.Add(t.PartNumber.Trim().ToUpperInvariant());
                if (UseName && !string.IsNullOrWhiteSpace(t.Name))
                    keys.Add(t.Name.Trim().ToUpperInvariant());
                return keys;
            };
        }
    }
}
