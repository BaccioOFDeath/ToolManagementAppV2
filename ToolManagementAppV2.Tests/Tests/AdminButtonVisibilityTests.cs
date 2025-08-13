using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ToolManagementAppV2;
using ToolManagementAppV2.Models.Domain;
using Xunit;

namespace ToolManagementAppV2.Tests.Tests
{
    public class AdminButtonVisibilityTests
    {
        [Fact]
        public void NonAdminUser_HidesAdminButtons()
        {
            if (Application.Current == null)
                new Application();

            Application.Current.Properties["CurrentUser"] = new UserModel { UserName = "user", IsAdmin = false };
            try
            {
                var window = new MainWindow();
                var dock = Assert.IsType<DockPanel>(window.Content);
                var stack = Assert.IsType<StackPanel>(dock.Children[0]);

                var restricted = new[] { "Tool Management", "Users", "Settings", "Import/Export" };
                bool anyVisible = stack.Children
                    .OfType<Button>()
                    .Where(b => restricted.Contains(b.Content?.ToString()))
                    .Any(b => b.Visibility == Visibility.Visible);

                Assert.False(anyVisible);
                window.Close();
            }
            finally
            {
                Application.Current.Properties.Remove("CurrentUser");
            }
        }
    }
}
