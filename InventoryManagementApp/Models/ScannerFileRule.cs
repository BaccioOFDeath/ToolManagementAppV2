using CommunityToolkit.Mvvm.ComponentModel;

namespace InventoryManagementApp.Models
{
    public class ScannerFileRule : ObservableObject
    {
        int _id;
        string _deviceId = string.Empty;
        string _sourcePath = string.Empty;
        string _destinationPath = string.Empty;
        string _pattern = string.Empty;

        public int Id
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }

        public string DeviceId
        {
            get => _deviceId;
            set => SetProperty(ref _deviceId, value);
        }

        public string SourcePath
        {
            get => _sourcePath;
            set => SetProperty(ref _sourcePath, value);
        }

        public string DestinationPath
        {
            get => _destinationPath;
            set => SetProperty(ref _destinationPath, value);
        }

        public string Pattern
        {
            get => _pattern;
            set => SetProperty(ref _pattern, value);
        }
    }
}
