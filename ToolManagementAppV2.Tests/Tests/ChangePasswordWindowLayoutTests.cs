using System.Windows.Controls;
using ToolManagementAppV2.Views.Pages;
using ToolManagementAppV2.Views.Windows;
using Xunit;

namespace ToolManagementAppV2.Tests.Tests
{
    public class ChangePasswordWindowLayoutTests
    {
        [Fact]
        public void ChangePasswordWindow_HasPasswordBoxes()
        {
            var win = new ChangePasswordWindow();
            Assert.IsType<PasswordBox>(win.FindName("NewPasswordBox"));
            Assert.IsType<PasswordBox>(win.FindName("ConfirmPasswordBox"));
            win.Close();
        }
    }
}
