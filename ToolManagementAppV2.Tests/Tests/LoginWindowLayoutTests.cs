using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ToolManagementAppV2.Models.Domain;
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
            Assert.True(VirtualizingStackPanel.GetIsVirtualizing(window.UsersListBox));
        }

        [Fact]
        public void UsersListBox_VirtualizesLargeCollections()
        {
            var window = new LoginWindow();
            window.UsersListBox.ItemsSource = Enumerable.Range(0, 1000)
                .Select(i => new User { UserID = i, UserName = $"User {i}" })
                .ToList();

            window.UsersListBox.Measure(new Size(800, 200));
            window.UsersListBox.Arrange(new Rect(0, 0, 800, 200));
            window.UsersListBox.UpdateLayout();

            Assert.NotNull(window.UsersListBox.ItemContainerGenerator.ContainerFromIndex(0));
            Assert.Null(window.UsersListBox.ItemContainerGenerator.ContainerFromIndex(999));
        }
    }
}
