using CommunityToolkit.Mvvm.ComponentModel;

namespace ToolManagementAppV2.Models
{
    public class StatCard : ObservableObject
    {
        string _title = string.Empty;
        string _value = string.Empty;

        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        public string Value
        {
            get => _value;
            set => SetProperty(ref _value, value);
        }
    }
}
