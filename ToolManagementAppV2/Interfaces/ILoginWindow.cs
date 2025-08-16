using System.Windows;

namespace ToolManagementAppV2.Interfaces
{
    /// <summary>
    /// Abstraction over the login window.
    /// </summary>
    public interface ILoginWindow
    {
        ILoginViewModel ViewModel { get; }
        bool? ShowDialog();
        Window Owner { get; set; }
        WindowStartupLocation WindowStartupLocation { get; set; }
    }
}
