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
        public bool UseItemNumber
        {
            get => _useItemNumber;
            set
            {
                if (SetProperty(ref _useItemNumber, value))
                    RefreshMappingReadiness();
            }
        }

        bool _usePartNumber;
        public bool UsePartNumber
        {
            get => _usePartNumber;
            set
            {
                if (SetProperty(ref _usePartNumber, value))
                    RefreshMappingReadiness();
            }
        }

        bool _useName;
        public bool UseName
        {
            get => _useName;
            set
            {
                if (SetProperty(ref _useName, value))
                    RefreshMappingReadiness();
            }
        }

        public int SelectedRuleCount => (UseItemNumber ? 1 : 0) + (UsePartNumber ? 1 : 0) + (UseName ? 1 : 0);
        public bool CanConfirmMapping => SelectedRuleCount > 0;
        public string MappingReadinessText => CanConfirmMapping
            ? $"Ready with {SelectedRuleCount} filename matching rule{(SelectedRuleCount == 1 ? string.Empty : "s")}."
            : "Choose at least one filename matching rule before continuing.";

        public IRelayCommand OkCommand { get; }
        public IRelayCommand CancelCommand { get; }

        public ImageImportMappingViewModel(Action onOk, Action onCancel)
        {
            OkCommand = new RelayCommand(onOk, () => CanConfirmMapping);
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

        void RefreshMappingReadiness()
        {
            OnPropertyChanged(nameof(SelectedRuleCount));
            OnPropertyChanged(nameof(CanConfirmMapping));
            OnPropertyChanged(nameof(MappingReadinessText));
            OkCommand.NotifyCanExecuteChanged();
        }
    }
}