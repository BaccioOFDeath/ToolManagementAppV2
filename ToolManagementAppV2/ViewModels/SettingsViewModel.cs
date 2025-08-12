using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using ToolManagementAppV2.Services.Core;

namespace ToolManagementAppV2.ViewModels
{
    internal class SettingsViewModel : ObservableObject
    {
        public SettingsViewModel()
        {
            ThemeOptions = new ObservableCollection<string> { "Light", "Dark" };
            _theme = ThemeOptions[0];
            TestDbCommand = new RelayCommand(TestDbConnection);
        }

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

        private string _connectionString;
        public string ConnectionString
        {
            get => _connectionString;
            set => SetProperty(ref _connectionString, value);
        }

        private string _theme;
        public string Theme
        {
            get => _theme;
            set => SetProperty(ref _theme, value);
        }

        public ObservableCollection<string> ThemeOptions { get; }

        public IRelayCommand TestDbCommand { get; }

        void TestDbConnection()
        {
            var db = new DatabaseService(ConnectionString);
            using var conn = db.CreateConnection();
        }
    }
}

