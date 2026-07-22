using System;
using System.IO;
using Xunit;

namespace InventoryManagementApp.Tests
{
    public class ManageItemsPageXamlTests
    {
        [Fact]
        public void DataGrid_UsesRowAndColumnVirtualization()
        {
            var xaml = ReadXaml();
            Assert.Contains("Style=\"{StaticResource VirtualizedDataGridStyle}\"", xaml);
            Assert.Contains("EnableColumnVirtualization=\"True\"", xaml);
        }

        [Fact]
        public void DataGrid_AllowsMultiSelection()
        {
            var xaml = ReadXaml();
            Assert.Contains("SelectionMode=\"Extended\"", xaml);
        }

        [Fact]
        public void DataGridRow_RightClickSelectHandlerIsWired()
        {
            var xaml = ReadXaml();
            Assert.Contains("Event=\"PreviewMouseRightButtonDown\"", xaml);
            Assert.Contains("Handler=\"DataGridRow_PreviewMouseRightButtonDown\"", xaml);
        }

        [Fact]
        public void DataGridRow_DoubleClickOpensSelectedItemDetails()
        {
            var xaml = ReadXaml();
            var codeBehind = ReadPageCodeBehind();

            Assert.Contains("Event=\"MouseDoubleClick\"", xaml, StringComparison.Ordinal);
            Assert.Contains("Handler=\"DataGridRow_MouseDoubleClick\"", xaml, StringComparison.Ordinal);
            Assert.Contains("GridContextMenuSelection.SelectRow(sender, e)", codeBehind, StringComparison.Ordinal);
            Assert.Contains("vm.ViewDetailsCommand.Execute(null)", codeBehind, StringComparison.Ordinal);
            Assert.Contains("UiActionGuard.Run(this, \"Item Details\"", codeBehind, StringComparison.Ordinal);
        }

        [Fact]
        public void DataGridRow_DoesNotClearImageBindingsWhenVirtualized()
        {
            var xaml = ReadXaml();
            var codeBehind = ReadPageCodeBehind();

            Assert.DoesNotContain("DataGridRow_Loaded", xaml);
            Assert.DoesNotContain("DataGridRow_Unloaded", xaml);
            Assert.DoesNotContain("ReleaseRowImage", codeBehind);
            Assert.DoesNotContain("img.Source = null", codeBehind);
        }

        [Fact]
        public void Columns_BindVisibilityToVisibleFields()
        {
            var xaml = ReadXaml();
            Assert.Contains("ShowItemNumber", xaml);
            Assert.Contains("ShowPartNumber", xaml);
            Assert.Contains("ShowName", xaml);
            Assert.Contains("ShowBrand", xaml);
            Assert.Contains("ShowQuantityOnHand", xaml);
            Assert.Contains("ShowLocation", xaml);
            Assert.Contains("ShowPrice", xaml);
            Assert.Contains("ShowNotes", xaml);
        }

        [Fact]
        public void CreationActionsAppearAtTopAndSelectedItemActionsStayWithTheDetailPane()
        {
            var xaml = ReadXaml();
            var newIndex = xaml.IndexOf("Content=\"New\"");
            var editIndex = xaml.IndexOf("Content=\"Edit\"");
            var dataGridIndex = xaml.IndexOf("<DataGrid");
            Assert.True(newIndex >= 0 && dataGridIndex >= 0 && newIndex < dataGridIndex);
            Assert.True(editIndex > dataGridIndex, "Selected-item actions should remain in the detail pane instead of being duplicated above the grid.");
        }

        [Fact]
        public void MobileCaptureButton_IsWiredToViewModelCommand()
        {
            var xaml = ReadXaml();

            Assert.Contains("Content=\"Mobile Capture\"", xaml, StringComparison.Ordinal);
            Assert.Contains("Command=\"{Binding OpenMobileCaptureCommand}\"", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void DeleteButton_IsVisibleAndWiredToSelectedItemCommand()
        {
            var xaml = ReadXaml();

            Assert.Contains("Content=\"Delete\"", xaml, StringComparison.Ordinal);
            Assert.Contains("Command=\"{Binding DeleteSelectedItemCommand}\"", xaml, StringComparison.Ordinal);
            Assert.Contains("CommandParameter=\"{Binding SelectedItem}\"", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void SummaryCards_ShowMissingImagesBesideEditAndPageSizeStats()
        {
            var xaml = ReadXaml();

            var pendingIndex = xaml.IndexOf("Text=\"Pending Edits\"", StringComparison.Ordinal);
            var missingIndex = xaml.IndexOf("Text=\"Missing Images\"", StringComparison.Ordinal);
            var pageSizeIndex = xaml.IndexOf("Text=\"Page Size\"", StringComparison.Ordinal);

            Assert.True(pendingIndex >= 0, "Pending edits card was not found.");
            Assert.True(missingIndex > pendingIndex, "Missing images card should appear after pending edits.");
            Assert.True(pageSizeIndex > missingIndex, "Page size card should appear after missing images.");
            Assert.Contains("Text=\"{Binding MissingImageCount}\"", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void Page_UsesCompactLaptopFriendlyDirectoryLayout()
        {
            var xaml = ReadXaml();

            Assert.Contains("<Style x:Key=\"DirectoryStatCard\" TargetType=\"Border\" BasedOn=\"{StaticResource PageHeaderStatCard}\"/>", xaml, StringComparison.Ordinal);
            Assert.Contains("Style=\"{StaticResource DirectoryStatValueText}\"", xaml, StringComparison.Ordinal);
            Assert.Contains("<ColumnDefinition Width=\"1.7*\" MinWidth=\"0\"/>", xaml, StringComparison.Ordinal);
            Assert.Contains("<ColumnDefinition Width=\"0.95*\" MinWidth=\"300\"/>", xaml, StringComparison.Ordinal);
            Assert.Contains("<pages:SearchBar Width=\"240\"", xaml, StringComparison.Ordinal);
            Assert.Contains("Content=\"New\"", xaml, StringComparison.Ordinal);
            Assert.Contains("Content=\"Mobile Capture\"", xaml, StringComparison.Ordinal);
        }

        private static string ReadXaml()
            => File.ReadAllText(Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "InventoryManagementApp", "Views", "Pages", "ManageItemsPage.xaml")));

        private static string ReadPageCodeBehind()
            => File.ReadAllText(Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "InventoryManagementApp", "Views", "Pages", "ManageItemsPage.xaml.cs")));
    }
}
