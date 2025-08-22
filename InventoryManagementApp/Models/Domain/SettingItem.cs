using CommunityToolkit.Mvvm.ComponentModel;

#nullable enable

namespace InventoryManagementApp.Models.Domain
{
    public class SettingItem : ObservableObject
    {
        private string _key = string.Empty;
        public string Key { get => _key; set => SetProperty(ref _key, value); }

        private string _value = string.Empty;
        public string Value { get => _value; set => SetProperty(ref _value, value); }
    }
}
