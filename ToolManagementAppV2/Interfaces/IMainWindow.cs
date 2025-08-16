using System.Windows;

namespace ToolManagementAppV2.Interfaces
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
