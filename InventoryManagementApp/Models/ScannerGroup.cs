using CommunityToolkit.Mvvm.ComponentModel;

namespace InventoryManagementApp.Models
{
    public class ScannerGroup : ObservableObject
    {
        int _id;
        string _name = string.Empty;

        public int Id
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }
    }
}
