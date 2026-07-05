using System;
using System.IO;
using Xunit;

namespace InventoryManagementApp.Tests.ViewModels
{
    public class UsersPageXamlTests
    {
        [Fact]
        public void UsersPage_UsesAdminWorkbenchSummariesAndCommands()
        {
            var xaml = ReadRepositoryFile("InventoryManagementApp", "Views", "Pages", "UsersPage.xaml");

            Assert.Contains("Users Workbench", xaml, StringComparison.Ordinal);
            Assert.Contains("Visible Users", xaml, StringComparison.Ordinal);
            Assert.Contains("Directory Filter", xaml, StringComparison.Ordinal);
            Assert.Contains("Selected Access", xaml, StringComparison.Ordinal);
            Assert.Contains("Security State", xaml, StringComparison.Ordinal);
            Assert.Contains("UserSearchText", xaml, StringComparison.Ordinal);
            Assert.Contains("SelectedUser.AccessSummary", xaml, StringComparison.Ordinal);
            Assert.Contains("SelectedUser.LockoutStatus", xaml, StringComparison.Ordinal);
            Assert.Contains("AddUserCommand", xaml, StringComparison.Ordinal);
            Assert.Contains("EditUserCommand", xaml, StringComparison.Ordinal);
            Assert.Contains("UploadUserPhotoCommand", xaml, StringComparison.Ordinal);
            Assert.Contains("ClearUserSearchCommand", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void UsersPage_PreservesDirectoryHooksAndHandoffActions()
        {
            var xaml = ReadRepositoryFile("InventoryManagementApp", "Views", "Pages", "UsersPage.xaml");
            var viewModel = ReadRepositoryFile("InventoryManagementApp", "ViewModels", "UserManagementViewModel.cs");

            Assert.Contains("UserStatCard", xaml, StringComparison.Ordinal);
            Assert.Contains("DesktopPaneHeader", xaml, StringComparison.Ordinal);
            Assert.Contains("DesktopPaneSubheader", xaml, StringComparison.Ordinal);
            Assert.Contains("DesktopNoteCard", xaml, StringComparison.Ordinal);
            Assert.Contains("Access And Security Handoff", xaml, StringComparison.Ordinal);
            Assert.Contains("Admin Next Step", xaml, StringComparison.Ordinal);
            Assert.Contains("UserEmptyStateTitle", xaml, StringComparison.Ordinal);
            Assert.Contains("No users match this filter", viewModel, StringComparison.Ordinal);
            Assert.Contains("UserRow_MouseDoubleClick", xaml, StringComparison.Ordinal);
            Assert.Contains("UserRow_PreviewMouseRightButtonDown", xaml, StringComparison.Ordinal);
            Assert.Contains("OpenSelectedUser_Click", xaml, StringComparison.Ordinal);
            Assert.Contains("CopySelectedUser_Click", xaml, StringComparison.Ordinal);
            Assert.Contains("ResetSelectedUser_Click", xaml, StringComparison.Ordinal);
            Assert.Contains("PrintUsers_Click", xaml, StringComparison.Ordinal);
            Assert.Contains("Admin desk ready", viewModel, StringComparison.Ordinal);
        }

        [Fact]
        public void UsersPage_LeavesRoomForDirectoryUserPhotos()
        {
            var xaml = ReadRepositoryFile("InventoryManagementApp", "Views", "Pages", "UsersPage.xaml");
            var gridIndex = xaml.IndexOf("x:Name=\"UsersDataGrid\"", StringComparison.Ordinal);

            Assert.True(gridIndex >= 0, "Users directory grid should exist.");
            var usersGrid = xaml.Substring(gridIndex);

            Assert.Contains("RowHeight=\"48\"", usersGrid, StringComparison.Ordinal);
            Assert.Contains("Width=\"34\" Height=\"34\"", usersGrid, StringComparison.Ordinal);
            Assert.Contains("UserPhotoPath=\"{Binding UserPhotoPath}\"", usersGrid, StringComparison.Ordinal);
        }

        static string ReadRepositoryFile(params string[] relativePathParts)
        {
            var directory = new DirectoryInfo(AppContext.BaseDirectory);
            while (directory != null && !File.Exists(Path.Combine(directory.FullName, "InventoryManagementApp.sln")))
                directory = directory.Parent;

            Assert.NotNull(directory);
            var path = Path.Combine(directory!.FullName, Path.Combine(relativePathParts));
            Assert.True(File.Exists(path), $"Expected repository file at {path}");
            return NormalizeLineEndings(File.ReadAllText(path));
        }
        static string NormalizeLineEndings(string text)
            => text.Replace("\r\n", "\n");

    }
}
