using System;
using System.IO;
using Xunit;

namespace InventoryManagementApp.Tests
{
    public class UsersPageResponsiveContractTests
    {
        [Fact]
        public void UsersPage_KeepsAccountSummaryCardsWrappedAndBounded()
        {
            var xaml = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "UsersPage.xaml");

            Assert.Contains("<WrapPanel Grid.Row=\"1\" Margin=\"0,0,0,6\">", xaml, StringComparison.Ordinal);
            Assert.Contains("<Setter Property=\"MinWidth\" Value=\"160\"/>", xaml, StringComparison.Ordinal);
            Assert.Contains("<Setter Property=\"MaxWidth\" Value=\"250\"/>", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("<Grid Grid.Row=\"1\" Margin=\"0,0,0,6\">", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("<ColumnDefinition Width=\"1.35*\"/>", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void UsersPage_AvoidsLargeFixedMinimumsInMainAccountSplit()
        {
            var xaml = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "UsersPage.xaml");

            Assert.Contains("<ColumnDefinition Width=\"1.65*\" MinWidth=\"0\"/>", xaml, StringComparison.Ordinal);
            Assert.Contains("<ColumnDefinition Width=\"0.95*\" MinWidth=\"300\"/>", xaml, StringComparison.Ordinal);
            Assert.Contains("<GridSplitter Grid.Column=\"1\" Width=\"6\"", xaml, StringComparison.Ordinal);
            Assert.Contains("<Border Grid.Column=\"0\" Style=\"{StaticResource Card}\" Padding=\"0\" MinWidth=\"0\">", xaml, StringComparison.Ordinal);
            Assert.Contains("<Border Grid.Column=\"2\" Style=\"{StaticResource Card}\" Padding=\"0\" MinWidth=\"0\">", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("<ColumnDefinition Width=\"2.15*\" MinWidth=\"620\"/>", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("<ColumnDefinition Width=\"1.05*\" MinWidth=\"360\"/>", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void UsersPage_EnablesDirectoryGridVirtualizationScrollingAndFullRowSelection()
        {
            var xaml = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "UsersPage.xaml");

            Assert.Contains("x:Name=\"UsersDataGrid\"", xaml, StringComparison.Ordinal);
            Assert.Contains("EnableRowVirtualization=\"True\"", xaml, StringComparison.Ordinal);
            Assert.Contains("EnableColumnVirtualization=\"True\"", xaml, StringComparison.Ordinal);
            Assert.Contains("SelectionMode=\"Single\"", xaml, StringComparison.Ordinal);
            Assert.Contains("SelectionUnit=\"FullRow\"", xaml, StringComparison.Ordinal);
            Assert.Contains("ScrollViewer.CanContentScroll=\"True\"", xaml, StringComparison.Ordinal);
            Assert.Contains("ScrollViewer.HorizontalScrollBarVisibility=\"Auto\"", xaml, StringComparison.Ordinal);
            Assert.Contains("ScrollViewer.VerticalScrollBarVisibility=\"Auto\"", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void UsersPage_BoundsSearchEmptyStateLoadingStateAndHandoffScrolling()
        {
            var xaml = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "UsersPage.xaml");

            Assert.Contains("<pages:SearchBar Width=\"300\"", xaml, StringComparison.Ordinal);
            Assert.Contains("MinWidth=\"220\"", xaml, StringComparison.Ordinal);
            Assert.Contains("IsEnabled=\"{Binding CanUseUserActions}\"", xaml, StringComparison.Ordinal);
            Assert.Contains("<Border Grid.Row=\"2\" MaxWidth=\"330\" MinHeight=\"120\" Margin=\"12\"", xaml, StringComparison.Ordinal);
            Assert.Contains("<MultiDataTrigger>", xaml, StringComparison.Ordinal);
            Assert.Contains("<Condition Binding=\"{Binding IsLoadingUsers}\" Value=\"False\"/>", xaml, StringComparison.Ordinal);
            Assert.Contains("<Border Grid.Row=\"2\" MaxWidth=\"360\" MinHeight=\"118\" Margin=\"12\"", xaml, StringComparison.Ordinal);
            Assert.Contains("<DataTrigger Binding=\"{Binding IsLoadingUsers}\" Value=\"True\">", xaml, StringComparison.Ordinal);
            Assert.Contains("<ScrollViewer Grid.Row=\"1\" VerticalScrollBarVisibility=\"Auto\" HorizontalScrollBarVisibility=\"Disabled\">", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("<Border Grid.Row=\"2\" Width=\"330\"", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("VerticalScrollBarVisibility=\"Hidden\"", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void UsersPage_UsesViewModelBackedStatusAndActionReadiness()
        {
            var xaml = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "UsersPage.xaml");

            Assert.Contains("UserDirectoryStatusText", xaml, StringComparison.Ordinal);
            Assert.Contains("UserFilterStatusText", xaml, StringComparison.Ordinal);
            Assert.Contains("SelectedAccessStatusText", xaml, StringComparison.Ordinal);
            Assert.Contains("SelectedSecurityStatusText", xaml, StringComparison.Ordinal);
            Assert.Contains("UserEmptyStateTitle", xaml, StringComparison.Ordinal);
            Assert.Contains("UserEmptyStateMessage", xaml, StringComparison.Ordinal);
            Assert.Contains("VisibleUserCount", xaml, StringComparison.Ordinal);
            Assert.Contains("CanUseUserActions", xaml, StringComparison.Ordinal);
            Assert.Contains("CanUseSelectedUserActions", xaml, StringComparison.Ordinal);
            Assert.Contains("CanPrintUsers", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void UsersPage_DisablesToolbarContextAndFooterActionsDuringBusyState()
        {
            var xaml = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "UsersPage.xaml");

            Assert.Contains("Content=\"Add User\" Command=\"{Binding AddUserCommand}\" IsEnabled=\"{Binding CanUseUserActions}\"", xaml, StringComparison.Ordinal);
            Assert.Contains("Content=\"Edit Access\" Command=\"{Binding EditUserCommand}\" IsEnabled=\"{Binding CanUseSelectedUserActions}\"", xaml, StringComparison.Ordinal);
            Assert.Contains("Content=\"Reset Password\" Click=\"ResetSelectedUser_Click\" IsEnabled=\"{Binding CanUseSelectedUserActions}\"", xaml, StringComparison.Ordinal);
            Assert.Contains("Content=\"Copy Handoff\" Click=\"CopySelectedUser_Click\" IsEnabled=\"{Binding CanUseSelectedUserActions}\"", xaml, StringComparison.Ordinal);
            Assert.Contains("Content=\"Print Directory\" Click=\"PrintUsers_Click\" IsEnabled=\"{Binding CanPrintUsers}\"", xaml, StringComparison.Ordinal);
            Assert.Contains("Header=\"Open User Detail\" Click=\"OpenSelectedUser_Click\" IsEnabled=\"{Binding CanUseSelectedUserActions}\"", xaml, StringComparison.Ordinal);
            Assert.Contains("Header=\"Print Current Directory\" Click=\"PrintUsers_Click\" IsEnabled=\"{Binding CanPrintUsers}\"", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void UsersPage_CodeBehindGuardsBusyRowActionsAndPrint()
        {
            var code = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "UsersPage.xaml.cs");

            Assert.Contains("private bool IsUserDirectoryBusy => ViewModel?.IsLoadingUsers == true;", code, StringComparison.Ordinal);
            Assert.Contains("TryRequireUserDirectoryReady", code, StringComparison.Ordinal);
            Assert.Contains("User rows are still loading", code, StringComparison.Ordinal);
            Assert.Contains("UserRow_MouseDoubleClick", code, StringComparison.Ordinal);
            Assert.Contains("e.Handled = true;", code, StringComparison.Ordinal);
            Assert.Contains("GridContextMenuSelection.SelectRow(sender, e);", code, StringComparison.Ordinal);
            Assert.Contains("ViewModel?.CanUseSelectedUserActions != true", code, StringComparison.Ordinal);
            Assert.Contains("!ViewModel.CanPrintUsers", code, StringComparison.Ordinal);
            Assert.Contains("active state, contact handoff details", code, StringComparison.Ordinal);
        }

        [Fact]
        public void UsersPage_PreservesPrimaryAccountActionsAndRowHandoff()
        {
            var xaml = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "UsersPage.xaml");

            Assert.Contains("AddUserCommand", xaml, StringComparison.Ordinal);
            Assert.Contains("EditUserCommand", xaml, StringComparison.Ordinal);
            Assert.Contains("ResetSelectedUser_Click", xaml, StringComparison.Ordinal);
            Assert.Contains("UploadUserPhotoCommand", xaml, StringComparison.Ordinal);
            Assert.Contains("CopySelectedUser_Click", xaml, StringComparison.Ordinal);
            Assert.Contains("PrintUsers_Click", xaml, StringComparison.Ordinal);
            Assert.Contains("OpenSelectedUser_Click", xaml, StringComparison.Ordinal);
            Assert.Contains("UserRow_PreviewMouseRightButtonDown", xaml, StringComparison.Ordinal);
        }

        private static string ReadRepoFile(params string[] parts)
        {
            var directory = AppContext.BaseDirectory;

            while (!string.IsNullOrEmpty(directory))
            {
                var candidate = Path.Combine(directory, Path.Combine(parts));
                if (File.Exists(candidate))
                    return File.ReadAllText(candidate);

                var parent = Directory.GetParent(directory);
                if (parent is null)
                    break;

                directory = parent.FullName;
            }

            throw new FileNotFoundException($"Could not find repository file: {Path.Combine(parts)}");
        }
    }
}