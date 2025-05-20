using CommunityToolkit.Mvvm.ComponentModel;

namespace ToolManagementAppV2.ViewModels.Settings
{
    internal class SettingsViewModel : ObservableObject
    {
        private string _applicationName;
        public string ApplicationName
        {
            get => _applicationName;
            set => SetProperty(ref _applicationName, value);
        }

        private string _companyLogoPath;
        public string CompanyLogoPath
        {
            get => _companyLogoPath;
            set => SetProperty(ref _companyLogoPath, value);
        }

        private int _defaultRentalDuration;
        public int DefaultRentalDuration
        {
            get => _defaultRentalDuration;
            set => SetProperty(ref _defaultRentalDuration, value);
        }
    }
}
