using System.Windows.Controls;
using DeviceManagementApp.Interfaces;

namespace DeviceManagementApp.Services
{
    public class NavigationService : INavigationService
    {
        public Frame? Frame { get; set; }

        public void Navigate(Page page)
        {
            Frame?.Navigate(page);
        }
    }
}
