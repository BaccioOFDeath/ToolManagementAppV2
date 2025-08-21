using System.Windows.Controls;
using InventoryManagementApp.Views.Pages;
using InventoryManagementApp.Views.Windows;
using Xunit;

namespace InventoryManagementApp.Tests.Tests
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
