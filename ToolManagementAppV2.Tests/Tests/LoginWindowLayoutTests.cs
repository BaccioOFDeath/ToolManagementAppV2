using System.Windows.Controls;
using ToolManagementAppV2;
using Xunit;

namespace ToolManagementAppV2.Tests.Tests
{
    public class LoginWindowLayoutTests
    {
        [Fact]
        public void UsersListBox_UsesHorizontalStackPanel()
        {
            var window = new LoginWindow();
            var panel = window.UsersListBox.ItemsPanel.LoadContent();
            var stackPanel = Assert.IsType<VirtualizingStackPanel>(panel);
            Assert.Equal(Orientation.Horizontal, stackPanel.Orientation);
            Assert.Equal(ScrollBarVisibility.Auto, ScrollViewer.GetHorizontalScrollBarVisibility(window.UsersListBox));
            Assert.Equal(ScrollBarVisibility.Disabled, ScrollViewer.GetVerticalScrollBarVisibility(window.UsersListBox));
        }
    }
}
