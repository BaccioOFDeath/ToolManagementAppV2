using CommunityToolkit.Mvvm.Input;
using System;
using InventoryManagementApp.Utilities.Helpers;

namespace InventoryManagementApp.ViewModels
{
    public class SetupWizardViewModel : ChangePasswordViewModel
    {
        readonly Action<string> _onGenerated;
        bool _isRandom;
        public bool IsRandom
        {
            get => _isRandom;
            private set => SetProperty(ref _isRandom, value);
        }

        string _applicationName = string.Empty;
        public string ApplicationName
        {
            get => _applicationName;
            set => SetProperty(ref _applicationName, value);
        }

        string _itemLabelSingular = "Item";
        public string ItemLabelSingular
        {
            get => _itemLabelSingular;
            set => SetProperty(ref _itemLabelSingular, value);
        }

        string _itemLabelPlural = "Items";
        public string ItemLabelPlural
        {
            get => _itemLabelPlural;
            set => SetProperty(ref _itemLabelPlural, value);
        }

        public IRelayCommand GenerateCommand { get; }

        public SetupWizardViewModel(Action onSave, Action onCancel, Action<string> onGenerated)
            : base(onSave, onCancel)
        {
            _onGenerated = onGenerated;
            GenerateCommand = new RelayCommand(() =>
            {
                var pwd = SecurityHelper.GeneratePassword(16);
                NewPassword = pwd;
                ConfirmPassword = pwd;
                IsRandom = true;
                _onGenerated(pwd);
            });
        }
    }
}
