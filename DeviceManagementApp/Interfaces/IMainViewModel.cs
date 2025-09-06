using System.Windows.Controls;
using CommunityToolkit.Mvvm.Input;

namespace DeviceManagementApp.Interfaces
{
    public interface IMainViewModel
    {
        Page? CurrentPage { get; }
        string CurrentPageTitle { get; }
        string WindowTitle { get; }
        IRelayCommand OpenDevicesCommand { get; }
        IRelayCommand ExitCommand { get; }
    }
}
