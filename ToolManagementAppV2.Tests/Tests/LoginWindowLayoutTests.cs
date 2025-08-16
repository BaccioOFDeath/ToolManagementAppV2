using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ToolManagementAppV2.Models.Domain;
using ToolManagementAppV2;
using ToolManagementAppV2.Services.Core;
using ToolManagementAppV2.Services.Users;
using ToolManagementAppV2.Services.Settings;
using ToolManagementAppV2.Services;
using ToolManagementAppV2.ViewModels;
using Xunit;

namespace ToolManagementAppV2.Tests.Tests
{
    public class LoginWindowLayoutTests
    {
        [Fact]
        public void UsersListBox_UsesHorizontalStackPanel()
        {
            var dbPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".db");
            using var db = new DatabaseService(dbPath);
            var userContext = new ApplicationUserContext();
            var userService = new UserService(db, userContext);
            var settingsService = new SettingsService(db);
            var vm = new LoginViewModel(userService, settingsService, new DialogService(), userContext);
            var window = new LoginWindow(vm);
            var panel = window.UsersListBox.ItemsPanel.LoadContent();
            var stackPanel = Assert.IsType<VirtualizingStackPanel>(panel);
            Assert.Equal(Orientation.Horizontal, stackPanel.Orientation);
            Assert.Equal(ScrollBarVisibility.Auto, ScrollViewer.GetHorizontalScrollBarVisibility(window.UsersListBox));
            Assert.Equal(ScrollBarVisibility.Disabled, ScrollViewer.GetVerticalScrollBarVisibility(window.UsersListBox));
            Assert.True(VirtualizingStackPanel.GetIsVirtualizing(window.UsersListBox));
            window.Close();
        }

        [Fact]
        public void UsersListBox_VirtualizesLargeCollections()
        {
            var dbPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".db");
            using var db = new DatabaseService(dbPath);
            var userContext = new ApplicationUserContext();
            var userService = new UserService(db, userContext);
            var settingsService = new SettingsService(db);
            var vm = new LoginViewModel(userService, settingsService, new DialogService(), userContext);
            var window = new LoginWindow(vm);
            window.UsersListBox.ItemsSource = Enumerable.Range(0, 1000)
                .Select(i => new User { UserID = i, UserName = $"User {i}" })
                .ToList();

            window.UsersListBox.Measure(new Size(800, 200));
            window.UsersListBox.Arrange(new Rect(0, 0, 800, 200));
            window.UsersListBox.UpdateLayout();

            Assert.NotNull(window.UsersListBox.ItemContainerGenerator.ContainerFromIndex(0));
            Assert.Null(window.UsersListBox.ItemContainerGenerator.ContainerFromIndex(999));
            window.Close();
        }
    }
}
