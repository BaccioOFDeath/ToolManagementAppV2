using System;
using CommunityToolkit.Mvvm.Input;
using InventoryManagementApp.Interfaces;

namespace InventoryManagementApp.ViewModels
{
    public class SetupWizardViewModel : ChangePasswordViewModel
    {
        readonly IFileDialogService _fileDialog;

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

        string _companyLogoPath = string.Empty;
        public string CompanyLogoPath
        {
            get => _companyLogoPath;
            set => SetProperty(ref _companyLogoPath, value);
        }

        public IRelayCommand BrowseCompanyLogoCommand { get; }

        public SetupWizardViewModel(IFileDialogService fileDialog, Action onSave, Action onCancel)
            : base(onSave, onCancel)
        {
            _fileDialog = fileDialog;
            BrowseCompanyLogoCommand = new RelayCommand(BrowseCompanyLogo);
        }

        void BrowseCompanyLogo()
        {
            var path = _fileDialog.OpenFile("Image Files|*.png;*.jpg;*.jpeg;*.bmp");
            if (!string.IsNullOrWhiteSpace(path))
                CompanyLogoPath = path;
        }
    }
}
