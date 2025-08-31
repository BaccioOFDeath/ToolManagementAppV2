using System;

namespace InventoryManagementApp.ViewModels
{
    public class SetupWizardViewModel : ChangePasswordViewModel
    {
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

        public SetupWizardViewModel(Action onSave, Action onCancel)
            : base(onSave, onCancel)
        {
        }
    }
}
