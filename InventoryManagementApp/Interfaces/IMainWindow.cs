using System.Windows;

namespace InventoryManagementApp.Interfaces
{
    /// <summary>
    /// Abstraction over the main application window.
    /// </summary>
    public interface IMainWindow
    {
        void Show();
        void Close();
        WindowState WindowState { get; set; }
        void Activate();
        void Focus();
    }
}
