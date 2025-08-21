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

        bool _useNameDescription;
        public bool UseNameDescription { get => _useNameDescription; set => SetProperty(ref _useNameDescription, value); }

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
                if (UseNameDescription && !string.IsNullOrWhiteSpace(t.NameDescription))
                    keys.Add(t.NameDescription.Trim().ToUpperInvariant());
                return keys;
            };
        }
    }
}
