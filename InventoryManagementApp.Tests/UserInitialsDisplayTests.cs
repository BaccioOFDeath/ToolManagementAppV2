using System;
using System.IO;
using Xunit;

namespace InventoryManagementApp.Tests
{
    public class UserInitialsDisplayTests
    {
        [Fact]
        public void UsersPage_ShowsInitialsWhenNoPhotoPath()
        {
            var xaml = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "UsersPage.xaml");
            Assert.Contains("<controls:UserAvatar UserName=\"{Binding UserName}\"", xaml);
            Assert.Contains("UserPhotoPath=\"{Binding UserPhotoPath}\"", xaml);
        }

        [Fact]
        public void UsersPage_ShowsDefaultPhotoWhenNameBlank()
        {
            var xaml = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "UsersPage.xaml");
            Assert.Contains("controls:UserAvatar UserName=\"{Binding UserName}\"", xaml);
            Assert.Contains("InitialsBrush", xaml);
        }

        [Fact]
        public void MainWindow_ShowsInitialsWhenNoPhotoPath()
        {
            var xaml = ReadRepoFile("InventoryManagementApp", "MainWindow.xaml");
            Assert.Contains("<controls:UserAvatar UserName=\"{Binding CurrentUserName}\"", xaml);
            Assert.Contains("UserPhotoPath=\"{Binding CurrentUserPhotoPath}\"", xaml);
        }

        [Fact]
        public void MainWindow_ShowsInitialsWhenPhotoPathMissing()
        {
            var xaml = ReadRepoFile("InventoryManagementApp", "MainWindow.xaml");
            Assert.Contains("CurrentUserPhotoPath", xaml);
            Assert.Contains("CurrentUserInitialsBrush", xaml);
        }

        [Fact]
        public void UsersEditWindow_ShowsInitialsWhenNoPhotoPath()
        {
            var xaml = ReadRepoFile("InventoryManagementApp", "Views", "Windows", "UsersEditWindow.xaml");
            Assert.Contains("EditingUser.UserPhotoPath, Converter={StaticResource ExistingFilePathToBoolConverter}", xaml);
            Assert.Contains("EditingUser.UserName, Converter={StaticResource NameToInitialsConverter}", xaml);
        }

        [Fact]
        public void LoginWindow_ShowsInitialsWhenNoPhotoPath()
        {
            var xaml = ReadRepoFile("InventoryManagementApp", "Views", "Windows", "LoginWindow.xaml");
            Assert.Contains("<controls:UserAvatar UserName=\"{Binding UserName}\"", xaml);
            Assert.Contains("UserPhotoPath=\"{Binding UserPhotoPath}\"", xaml);
        }

        [Fact]
        public void LoginWindow_ShowsPhotoWhenPhotoPathExists()
        {
            var xaml = ReadRepoFile("InventoryManagementApp", "Views", "Windows", "LoginWindow.xaml");
            Assert.Contains("controls:UserAvatar", xaml);
            Assert.Contains("InitialsBrush", xaml);
        }

        private static string ReadRepoFile(params string[] parts)
            => File.ReadAllText(Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", Path.Combine(parts))));
    }
}
