using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using Xunit;

namespace InventoryManagementApp.Tests
{
    public class ManageItemsPageXamlTests
    {
        [Fact]
        public void DataGrid_UsesRowAndColumnVirtualization()
        {
            Exception? threadEx = null;
            var thread = new Thread(() =>
            {
                try
                {
                    var app = new Application();
                    app.Resources.MergedDictionaries.Add(new ResourceDictionary
                    {
                        Source = new Uri("pack://application:,,,/InventoryManagementApp;component/Resources/Styles.xaml", UriKind.Absolute)
                    });

                    var path = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "InventoryManagementApp", "Views", "Pages", "ManageItemsPage.xaml"));
                    var xaml = File.ReadAllText(path);
                    xaml = Regex.Replace(xaml, @"x:Class=""[^""]*""\s*", string.Empty);

                    var page = (Page)XamlReader.Parse(xaml);
                    var dataGrid = FindVisualChild<DataGrid>(page) ?? throw new InvalidOperationException("DataGrid not found");

                    Assert.True(VirtualizingStackPanel.GetIsVirtualizing(dataGrid));
                    Assert.Equal(VirtualizationMode.Recycling, VirtualizingStackPanel.GetVirtualizationMode(dataGrid));
                    Assert.True(dataGrid.EnableRowVirtualization);
                    Assert.True(dataGrid.EnableColumnVirtualization);
                }
                catch (Exception ex)
                {
                    threadEx = ex;
                }
                finally
                {
                    Application.Current?.Shutdown();
                }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();

            if (threadEx != null) throw threadEx;
        }

        [Fact]
        public void DataGrid_AllowsMultiSelection()
        {
            var path = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "InventoryManagementApp", "Views", "Pages", "ManageItemsPage.xaml"));
            var xaml = File.ReadAllText(path);
            Assert.Contains("SelectionMode=\"Extended\"", xaml);

            xaml = Regex.Replace(xaml, "x:Class=\"[^\"]*\"\\s*", string.Empty);
            var page = (Page)XamlReader.Parse(xaml);
            var dataGrid = FindVisualChild<DataGrid>(page) ?? throw new InvalidOperationException("DataGrid not found");
            Assert.Equal(DataGridSelectionMode.Extended, dataGrid.SelectionMode);
        }

        [Fact]
        public void DataGridRow_RightClickSelectHandlerIsWired()
        {
            var path = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "InventoryManagementApp", "Views", "Pages", "ManageItemsPage.xaml"));
            var xaml = File.ReadAllText(path);
            xaml = Regex.Replace(xaml, "x:Class=\"[^\"]*\"\\s*", string.Empty);

            var page = (Page)XamlReader.Parse(xaml);
            var dataGrid = FindVisualChild<DataGrid>(page) ?? throw new InvalidOperationException("DataGrid not found");
            var rowStyle = dataGrid.RowStyle ?? throw new InvalidOperationException("RowStyle not found");
            var eventSetter = rowStyle.Setters.OfType<EventSetter>().FirstOrDefault(es => es.Event == UIElement.PreviewMouseRightButtonDownEvent);
            Assert.NotNull(eventSetter);
            Assert.Equal("DataGridRow_PreviewMouseRightButtonDown", eventSetter!.Handler);
        }

        [Fact]
        public void Columns_BindVisibilityToVisibleFields()
        {
            var path = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "InventoryManagementApp", "Views", "Pages", "ManageItemsPage.xaml"));
            var xaml = File.ReadAllText(path);
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
            var path = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "InventoryManagementApp", "Views", "Pages", "ManageItemsPage.xaml"));
            var xaml = File.ReadAllText(path);
            Assert.DoesNotContain("Grid.Row=\"3\"", xaml);
            var editIndex = xaml.IndexOf("Content=\"Edit\"");
            var dataGridIndex = xaml.IndexOf("<DataGrid");
            Assert.True(editIndex >= 0 && dataGridIndex >= 0 && editIndex < dataGridIndex);
        }

        private static T? FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T t)
                    return t;
                var result = FindVisualChild<T>(child);
                if (result != null)
                    return result;
            }
            return null;
        }
    }
}
