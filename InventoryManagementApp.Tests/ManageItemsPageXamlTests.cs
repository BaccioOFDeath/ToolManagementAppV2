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
        public void ActionButtons_AppearAtTop()
        {
            var xaml = ReadXaml();
            var editIndex = xaml.IndexOf("Content=\"Edit\"");
            var dataGridIndex = xaml.IndexOf("<DataGrid");
            Assert.True(editIndex >= 0 && dataGridIndex >= 0 && editIndex < dataGridIndex);
        }

        [Fact]
        public void MobileCaptureButton_IsWiredToViewModelCommand()
        {
            var xaml = ReadXaml();

            Assert.Contains("Content=\"Mobile Capture\"", xaml, StringComparison.Ordinal);
            Assert.Contains("Command=\"{Binding OpenMobileCaptureCommand}\"", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void Page_UsesCompactLaptopFriendlyDirectoryLayout()
        {
            var xaml = ReadXaml();

            Assert.Contains("<Setter Property=\"MinHeight\" Value=\"54\"/>", xaml, StringComparison.Ordinal);
            Assert.Contains("Style=\"{StaticResource StatisticValueTextBlock}\"", xaml, StringComparison.Ordinal);
            Assert.Contains("<ColumnDefinition Width=\"3.4*\" MinWidth=\"620\"/>", xaml, StringComparison.Ordinal);
            Assert.Contains("<ColumnDefinition Width=\"1*\" MinWidth=\"250\"/>", xaml, StringComparison.Ordinal);
            Assert.Contains("<pages:SearchBar Width=\"260\"", xaml, StringComparison.Ordinal);
            Assert.Contains("Padding=\"6,3\"", xaml, StringComparison.Ordinal);
        }

        private static string ReadXaml()
            => File.ReadAllText(Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "InventoryManagementApp", "Views", "Pages", "ManageItemsPage.xaml")));

        private static string ReadPageCodeBehind()
            => File.ReadAllText(Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "InventoryManagementApp", "Views", "Pages", "ManageItemsPage.xaml.cs")));
    }
}
